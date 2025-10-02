using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddChatClient(services => new ChatClientBuilder(
    new AzureOpenAIClient(
        new Uri("https://azure-ai-foundry-adv-1.cognitiveservices.azure.com/"),
        new DefaultAzureCredential()
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
app.MapControllers();

app.Run("http://localhost:5170");
