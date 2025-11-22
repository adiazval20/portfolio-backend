using System;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace PortfolioAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AboutController : ControllerBase
{
    private readonly string _endPoint;

    public AboutController(IConfiguration config)
    {
        _endPoint = config["McpEndpoint"] ?? "WTF";
    }

    [HttpGet("")]
    public async Task<IActionResult> GetTools()
    {
        return Ok(_endPoint);
    } 
}