using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.AI;
using PortfolioAPI.Services;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
var azureAiUrl = config["AzureAI:Url"] ?? "";
var azureAiKey = config["AzureAI:Key"] ?? "";
var allowedOrigin = config["AllowedOrigin"] ?? "";

// Add services to the container.
builder.Services.AddScoped<IMcpService, McpService>();
builder.Services.AddScoped<IChatService, ChatService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins(allowedOrigin)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddChatClient(services => new ChatClientBuilder(
    new AzureOpenAIClient(
        new Uri(azureAiUrl),
        new AzureKeyCredential(azureAiKey)
    ).GetChatClient("gpt-4.1").AsIChatClient()
)
.UseFunctionInvocation()
.Build());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.MapControllers();

app.Run();
