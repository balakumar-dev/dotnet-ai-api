using DotNetAI.Core.Chunking;
using DotNetAI.Core.Interfaces;
using DotNetAI.Core.Prompts;
using DotNetAI.Core.Services;
using DotNetAI.Infrastructure.AI;
using DotNetAI.Infrastructure.Documents;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.Configure<AiOptions>(
    builder.Configuration.GetSection(AiOptions.SectionName));
builder.Services.Configure<OpenAIOptions>(
    builder.Configuration.GetSection(OpenAIOptions.SectionName));
builder.Services.Configure<GeminiOptions>(
    builder.Configuration.GetSection(GeminiOptions.SectionName));
builder.Services.Configure<KnowledgeOptions>(options =>
{
    builder.Configuration.GetSection(KnowledgeOptions.SectionName).Bind(options);
    if (!Path.IsPathRooted(options.DocumentsPath))
    {
        options.DocumentsPath = Path.Combine(builder.Environment.ContentRootPath, options.DocumentsPath);
    }
});

builder.Services.AddSingleton<IChatPromptBuilder, ChatPromptBuilder>();
builder.Services.AddSingleton<IDocumentChunker, DocumentChunker>();
builder.Services.AddSingleton<IVectorStore, InMemoryVectorStore>();
builder.Services.AddSingleton<IEmbeddingService, GeminiEmbeddingService>();
builder.Services.AddSingleton<IFileFormatExtractor, PlainTextDocumentExtractor>();
builder.Services.AddSingleton<IFileFormatExtractor, PdfDocumentExtractor>();
builder.Services.AddSingleton<IFileFormatExtractor, WordDocumentExtractor>();
builder.Services.AddSingleton<IDocumentTextExtractor, CompositeDocumentTextExtractor>();
builder.Services.AddSingleton<IDocumentIngestionService, DocumentIngestionService>();
builder.Services.AddSingleton<IKnowledgeSearchService, KnowledgeSearchService>();
builder.Services.AddHostedService<KnowledgeBaseSeeder>();
builder.Services.AddSingleton<OpenAIService>();
builder.Services.AddSingleton<GeminiService>();
builder.Services.AddSingleton<IAIService>(sp =>
{
    string provider = sp.GetRequiredService<IOptions<AiOptions>>().Value.Provider;

    return provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase)
        ? sp.GetRequiredService<GeminiService>()
        : sp.GetRequiredService<OpenAIService>();
});
builder.Services.AddSingleton<IRagChatService, RagChatService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("DevCors");
}

app.MapControllers();

app.Run();
