using System;
using Microsoft.AspNetCore.Mvc;
using PortfolioMCP.Contracts;

namespace PortfolioMCP.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilesController(ILogger<FilesController> _logger, IRagService _rag) : ControllerBase
{
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file, [FromForm] string? docId, CancellationToken ct)
    {
        if (file is null || file.Length == 0) return BadRequest("Attach a PDF as 'file'.");

        docId ??= Path.GetFileNameWithoutExtension(file.FileName);

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, ct);
        ms.Position = 0;

        await _rag.UpsertPdfAsync(docId, ms, 900, ct);

        return Ok(new { ok = true, docId });
    }

    [HttpPost("answer")]
    public async Task<IActionResult> Answer([FromBody] QuestionRequestDTO body, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(body.Question))
            return BadRequest("Missing 'question'.");

        var res = await _rag.AnswerAsync(body.Question, body.DocId);

        return Ok(new AnswerDTO (res.Text, body.Question));
    }

    
}
