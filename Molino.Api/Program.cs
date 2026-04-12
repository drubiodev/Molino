using System.Threading.Channels;
using Molino.Api.BackgroundServices;
using Molino.Api.Endpoints;
using Molino.Core.Configs;
using Molino.Core.Models;
using Molino.Core.Pipeline;
using Molino.Core.Services;
using Molino.Core.Stores;
using Molino.Infrastructure.Services;
using Molino.Infrastructure.Stores;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// --- Configuration binding ---
builder.Services.Configure<PipelineConfig>(builder.Configuration.GetSection("Pipeline"));
builder.Services.Configure<CosmosDbConfig>(builder.Configuration.GetSection("CosmosDb"));
builder.Services.Configure<AdoConfig>(builder.Configuration.GetSection("AzureDevOps"));
builder.Services.Configure<LlmConfig>(builder.Configuration.GetSection("Llm"));

// --- Core services ---
builder.Services.AddScoped<PipelineOrchestrator>();

// --- Infrastructure services ---
builder.Services.AddScoped<IWorkItemService, AdoService>();
builder.Services.AddScoped<IGitService, GitService>();
builder.Services.AddSingleton<IExecutionStore, CosmosExecutionStore>();
builder.Services.AddSingleton<ILlmService, LlmService>();
builder.Services.AddScoped<ISpecService, SpecService>();
builder.Services.AddScoped<ICopilotService, CopilotService>();

// --- Background processing channel ---
var channel = Channel.CreateBounded<ImplementationRequest>(new BoundedChannelOptions(50)
{
  FullMode = BoundedChannelFullMode.Wait
});
builder.Services.AddSingleton(channel);
builder.Services.AddHostedService<PipelineWorker>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapPipelineEndpoints();

app.Run();
