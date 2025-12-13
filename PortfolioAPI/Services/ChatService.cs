using System;
using System.Text;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

namespace PortfolioAPI.Services;

public class ChatService : IChatService
{
    private readonly ILogger<ChatService> _logger;
    private readonly IMcpService _mcpService;
    private readonly IChatClient _chatClient;

    public ChatService(ILogger<ChatService> logger, IMcpService mcpService, IChatClient chatClient)
    {
        _logger = logger;
        _mcpService = mcpService;
        _chatClient = chatClient;
    }

    public async Task<string> Chat(string message)
    {
        var mcpClient = await _mcpService.GetMcpClient();

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
}
