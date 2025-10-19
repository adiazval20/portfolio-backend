using System;
using System.ComponentModel;
using ModelContextProtocol.Server;
using PortfolioMCP.Contracts;

namespace PortfolioMCP.Tools;

[McpServerToolType]
public class RagTools(IRagService _rag)
{
    [McpServerTool, Description("Ask a question about Andy's resume")]
    public async Task<string> Ask(QuestionRequestDTO request, CancellationToken ct)
    {
        var res = await _rag.AnswerAsync(request.Question, request.DocId);
        return res.Text;
    }
}
