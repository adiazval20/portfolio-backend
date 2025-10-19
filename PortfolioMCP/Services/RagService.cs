using System;
using System.Text;
using System.Text.RegularExpressions;
using OpenAI.Chat;
using OpenAI.Embeddings;
using PortfolioMCP.Contracts;
using PortfolioMCP.Domain;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using UglyToad.PdfPig;

namespace PortfolioMCP.Services;

public sealed class RagService : IRagService
{
    private readonly QdrantClient _qdrant;
    private readonly EmbeddingClient _emb;
    private readonly string _collection;
    private readonly int _dim;
    private readonly string _openAiKey;

    public RagService(IConfiguration cfg)
    {
        var url = cfg["Qdrant:Url"]!;
        var key = cfg["Qdrant:ApiKey"]!;
        var model = cfg["OpenAI:EmbeddingModel"] ?? "text-embedding-3-small";
        _openAiKey = cfg["OpenAI:ApiKey"]!;

        _collection = cfg["Qdrant:Collection"] ?? "chunks";
        _dim = int.Parse(cfg["Qdrant:VectorSize"] ?? "1536");

        _qdrant = new QdrantClient(host: url, apiKey: key, https: true);
        _emb = new EmbeddingClient(model, _openAiKey);
    }

    public async Task<Answer> AnswerAsync(string question, string? docId = null)
    {
        var vectors = await AskForVectorsAsync(question, docId);
        var chat = new ChatClient("gpt-4o-mini", _openAiKey);

        var sb = new StringBuilder();
        foreach (var h in vectors)
        {
            var text = h.Payload.TryGetValue("text", out var t) ? t.StringValue : "";
            sb.AppendLine(text.Length > 700 ? text[..700] + " …" : text);
        }
        var context = sb.ToString();

        var completion = await chat.CompleteChatAsync($"You are a helpful assistant. Answer strictly from the provided context. If the answer isn’t in the context, say you don’t know and politely ask to try another question. Question: {question}\n\nContext:\n{context}\n\nReturn a concise answer.");
        var answer = completion.Value.Content[0].Text;

        return new Answer(answer);
    }

    public async Task EnsureCollectionAsync()
    {
        if (!await _qdrant.CollectionExistsAsync(_collection))
        {
            await _qdrant.CreateCollectionAsync(_collection, new VectorParams
            {
                Size = (uint)_dim,
                Distance = Distance.Cosine
            });
        }
    }

    public IEnumerable<(string text, int page, int idx)> ReadPdfChunks(Stream pdf, int chunkChars = 900)
    {
        using var doc = PdfDocument.Open(pdf);
        int idx = 0;
        for (int p = 1; p <= doc.NumberOfPages; p++)
        {
            var raw = doc.GetPage(p).Text ?? string.Empty;
            var norm = Regex.Replace(raw, @"\s+", " ").Trim();
            for (int start = 0; start < norm.Length; start += chunkChars)
            {
                var len = Math.Min(chunkChars, norm.Length - start);
                yield return (norm.Substring(start, len), p, idx++);
            }
        }
    }

    public async Task UpsertPdfAsync(string docId, Stream pdfStream, int chunkChars = 900, CancellationToken ct = default)
    {
        await EnsureCollectionAsync();

        var chunks = ReadPdfChunks(pdfStream, chunkChars).ToList();
        if (chunks.Count == 0) throw new InvalidOperationException("No text found; add OCR if scanned.");

        var pointTasks = chunks
            .Select(async (c, i) =>
            {
                var v = await GetEmbeddingAsync(c.text);
                return new PointStruct
                {
                    Id = new PointId { Uuid= Guid.NewGuid().ToString() },
                    Vectors = v,
                    Payload =
                    {
                        ["doc_id"] = docId,
                        ["page"] = c.page,
                        ["chunk_index"] = c.idx,
                        ["text"] = c.text
                    }
                };
            })
            .ToList();

        var points = (await Task.WhenAll(pointTasks)).ToList();
        await _qdrant.UpsertAsync(_collection, points, cancellationToken: ct);
    }

    private async Task<IReadOnlyList<ScoredPoint>> AskForVectorsAsync(string question, string? docId = null, int topK = 5, CancellationToken ct = default)
    {
        var q = await GetEmbeddingAsync(question);
        Filter? filter = null;
        if (!string.IsNullOrWhiteSpace(docId))
            filter = Conditions.MatchKeyword("doc_id", docId);

        return await _qdrant.SearchAsync(_collection, q, limit: (uint)Math.Clamp(topK, 1, 15), filter: filter, cancellationToken: ct);
    }

    private async Task<float[]> GetEmbeddingAsync(string text)
    {
        var resp = await _emb.GenerateEmbeddingAsync(text);
        return resp.Value.ToFloats().ToArray();
    }
}
