using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using PortfolioAPI.Services;

namespace PortfolioAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class ChatController
{
    private readonly ILogger<ChatController> _logger;
    private readonly IChatService _chatService;
    private readonly IMcpService _mcpService;

    public ChatController(ILogger<ChatController> logger, IChatService chatService, IMcpService mcpService)
    {
        _logger = logger;
        _chatService = chatService;
        _mcpService = mcpService;
    }

    [HttpPost(Name = "Chat")]
    public async Task<string> Chat([FromBody] string message)
    {
        var response = await _chatService.Chat(message);
        return response;
    }

    [HttpGet("tools")]
    public async Task<JsonResult> GetTools()
    {
        var tools = await _mcpService.GetTools();
        return new JsonResult(tools);
    }
}
