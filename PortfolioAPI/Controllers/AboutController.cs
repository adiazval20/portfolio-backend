using System;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace PortfolioAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AboutController
{
    [HttpGet("")]
    public async Task<IActionResult> GetTools()
    {
        return new OkResult();
    } 
}