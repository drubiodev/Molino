using System.Threading.Channels;
using Molino.Api.BackgroundServices;
using Molino.Api.Endpoints;
using Molino.Core.Configs;
using Molino.Core.Models;
using Molino.Infrastructure.Stores;

var builder = WebApplication.CreateBuilder(args);

// --- Configuration binding ---
builder.Services.Configure<CosmosDbConfig>(builder.Configuration.GetSection("CosmosDb"));

// --- Infrastructure services ---
builder.Services.AddSingleton<IExecutionStore, CosmosExecutionStore>();

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
