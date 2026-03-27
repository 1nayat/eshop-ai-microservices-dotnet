using System.ClientModel;
using Microsoft.Extensions.AI;
using Microsoft.OpenApi;
using OpenAI;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<CatalogDbContext>(connectionName: "catalogdb");

builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<ProductAIService>();

// 🔥 Swagger 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Catalog API",
        Version = "v1",
        Description = "Product Catalog with AI Search (.NET Aspire)"
    });
});

#region AI Client Configuration

// Add AI Chat Client
var credential = new ApiKeyCredential(builder.Configuration["GitHubModels:Token"]);
var options = new OpenAIClientOptions()
{
    Endpoint = new Uri("https://models.github.ai/inference")
};

var openAiClient = new OpenAIClient(credential, options);

var chatClient =
    openAiClient.GetChatClient("openai/gpt-4o-mini").AsIChatClient();

var embeddingGenerator =
    openAiClient.GetEmbeddingClient("openai/text-embedding-3-small").AsIEmbeddingGenerator();

builder.Services.AddChatClient(chatClient);
builder.Services.AddEmbeddingGenerator(embeddingGenerator);

builder.AddQdrantClient("vectordb");
builder.Services.AddQdrantCollection<ulong, ProductVector>("product-vectors");

#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapDefaultEndpoints();

app.UseHttpsRedirection();

// 🔥 Swagger middleware (only dev)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalog API v1");
        options.RoutePrefix = "swagger"; // optional
    });
}

app.UseMigration();

app.MapProductEndpoints();

app.Run();