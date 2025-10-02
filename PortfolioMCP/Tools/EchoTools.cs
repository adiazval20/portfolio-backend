using System;
using System.ComponentModel;
using ModelContextProtocol.Server;

namespace PortfolioMCP.Tools;

public class EchoTools
{
    [McpServerTool, Description("Echoes the message back to the client.")]
    public static string Echo(string message) => $"hello {message}";
    
    [McpServerTool, Description("Echoes in reverse the message sent by the client.")]
    public static string ReverseEcho(string message) => new string(message.Reverse().ToArray());
}
