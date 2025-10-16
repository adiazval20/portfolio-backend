using System;

namespace PortfolioMCP.Contracts;

public sealed record QuestionRequestDTO(string Question, string? DocId, int TopK = 5);