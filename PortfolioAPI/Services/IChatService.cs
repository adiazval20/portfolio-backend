using System;
using ModelContextProtocol.Client;

namespace PortfolioAPI.Services;

public interface IChatService
{
    Task<string> Chat(string message);
}
