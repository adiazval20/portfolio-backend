using System;
using ModelContextProtocol.Client;

namespace PortfolioAPI.Services;

public class McpService : IMcpService
{
    private readonly string _endPoint;
    public McpService(IConfiguration config)
    {
        _endPoint = config["McpEndpoint"] ?? throw new Exception("The MCP Endpoint shouldn't be null");
    }
    public Task<IMcpClient> GetMcpClient()
    {
        var options = new SseClientTransportOptions
        {
            Endpoint = new Uri(_endPoint)
        };
        var transport = new SseClientTransport(options);
        var mcpClient = McpClientFactory.CreateAsync(transport);
        return mcpClient;
    }

    public async Task<IList<McpClientTool>> GetTools()
    {
        var mcpClient = await GetMcpClient();
        return await mcpClient.ListToolsAsync();
    }
}
