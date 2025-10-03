using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

namespace PortfolioAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class ChatController
{
    private readonly ILogger<ChatController> _logger;
    private readonly IChatClient _chatClient;

    private readonly string _endPoint = "https://localhost:7262/sse";

    public ChatController(ILogger<ChatController> logger, IChatClient chatClient)
    {
        _logger = logger;
        _chatClient = chatClient;
    }

    [HttpPost(Name = "Chat")]
    public async Task<string> Chat([FromBody] string message)
    {
        var mcpClient = await GetMcpClient();

        var tools = await mcpClient.ListToolsAsync();
        var messages = new List<ChatMessage>
        {
            new(ChatRole.User, message)
        };

        List<ChatResponseUpdate> updates = [];
        StringBuilder result = new StringBuilder();

        await foreach (var update in _chatClient.GetStreamingResponseAsync(messages, new() { Tools = [.. tools] }))
        {
            result.Append(update);
            updates.Add(update);
        }

        messages.AddMessages(updates);
        return result.ToString();
    }

    [HttpGet("tools")]
    public async Task<JsonResult> GetTools()
    {
        var mcpClient = await GetMcpClient();
        var tools = await mcpClient.ListToolsAsync();
        return new JsonResult(tools);
    } 
    
    private async Task<IMcpClient> GetMcpClient() {
        var options = new SseClientTransportOptions
        {
            Endpoint = new Uri(_endPoint)
        };
        var transport = new SseClientTransport(options);
        var mcpClient = await McpClientFactory.CreateAsync(transport);
        return mcpClient;
    }
}
