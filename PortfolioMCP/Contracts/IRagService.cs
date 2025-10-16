using System;
using PortfolioMCP.Domain;

namespace PortfolioMCP.Contracts;

public interface IRagService
{
    Task EnsureCollectionAsync();
    IEnumerable<(string text, int page, int idx)> ReadPdfChunks(Stream pdf, int chunkChars = 900);
    Task UpsertPdfAsync(string docId, Stream pdfStream, int chunkChars = 900, CancellationToken ct = default);
    Task<Answer> AnswerAsync(string question, string? docId = null);
}
