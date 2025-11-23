using System;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace PortfolioAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AboutController : ControllerBase
{
    private readonly string _parameterForTest;

    public AboutController(IConfiguration config)
    {
        _parameterForTest = config["AllowedOrigin"] ?? "WTF";
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        return Ok(_parameterForTest);
    } 
}