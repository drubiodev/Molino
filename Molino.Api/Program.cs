using Molino.Core.Configs;
using Molino.Infrastructure.Stores;

var builder = WebApplication.CreateBuilder(args);

// --- Configuration binding ---
builder.Services.Configure<CosmosDbConfig>(builder.Configuration.GetSection("CosmosDb"));

// --- Infrastructure services ---
builder.Services.AddSingleton<IExecutionStore, CosmosExecutionStore>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
