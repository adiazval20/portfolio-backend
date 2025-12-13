using System;
using ModelContextProtocol.Client;

namespace PortfolioAPI.Services;

public interface IMcpService
{
    Task<IMcpClient> GetMcpClient();
    Task<IList<McpClientTool>> GetTools();
}
