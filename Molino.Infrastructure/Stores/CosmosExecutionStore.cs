using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Molino.Core.Configs;
using Molino.Core.Models;
using Molino.Core.Stores;

namespace Molino.Infrastructure.Stores;

public sealed class CosmosExecutionStore : IExecutionStore
{
  private readonly CosmosClient _client;
  private readonly CosmosDbConfig _config;
  private readonly ILogger<CosmosExecutionStore> _logger;
  private Container? _container;
  private readonly SemaphoreSlim _initLock = new(1, 1);

  public CosmosExecutionStore(IOptions<CosmosDbConfig> options, ILogger<CosmosExecutionStore> logger)
  {
    _logger = logger;
    _config = options.Value;
    _client = new CosmosClient(_config.Account, _config.Key);
  }

  private async Task<Container> GetContainerAsync()
  {
    if (_container is not null)
      return _container;

    await _initLock.WaitAsync();
    try
    {
      if (_container is not null)
        return _container;

      _logger.LogInformation("Initializing Cosmos DB database '{Database}' and container '{Container}'",
          _config.Database, _config.Container);

      var db = await _client.CreateDatabaseIfNotExistsAsync(_config.Database);
      var container = await db.Database.CreateContainerIfNotExistsAsync(
          _config.Container, _config.PartitionKeyPath);

      _container = container.Container;
      return _container;
    }
    finally
    {
      _initLock.Release();
    }
  }

  public async Task<ExecutionRecord> CreateAsync(ExecutionRecord record, CancellationToken ct = default)
  {
    var container = await GetContainerAsync();
    var response = await container.CreateItemAsync(record, new PartitionKey(record.WorkItemId), cancellationToken: ct);
    return response.Resource;
  }

  public async Task<ExecutionRecord?> GetAsync(ImplementationRequest request, CancellationToken ct = default)
  {
    var container = await GetContainerAsync();

    try
    {
      var response = await container.ReadItemAsync<ExecutionRecord>(request.ExecutionId, new PartitionKey(request.WorkItemId), cancellationToken: ct);
      return response.Resource;
    }
    catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
    {
      return null;
    }
  }

  public Task<IReadOnlyList<ExecutionRecord>> GetByWorkItemAsync(int workItemId, CancellationToken ct = default)
  {
    throw new NotImplementedException();
  }

  public async Task<ExecutionRecord?> UpdateStatusAsync(ImplementationRequest request, ExecutionStatus status, CancellationToken ct = default)
  {
    var record = await GetAsync(request, ct);
    if (record is null)
      return null;

    return await UpdateAsync(record with { Status = status }, ct);
  }

  public async Task<ExecutionRecord?> UpdateStepAsync(ImplementationRequest request, PipelineStep step, CancellationToken ct = default)
  {
    var record = await GetAsync(request, ct);
    if (record is null)
      return null;

    return await UpdateAsync(record with { CurrentStep = step }, ct);
  }

  public async Task<ExecutionRecord> UpdateAsync(ExecutionRecord record, CancellationToken ct = default)
  {
    var container = await GetContainerAsync();
    var response = await container.UpsertItemAsync(record, new PartitionKey(record.WorkItemId), cancellationToken: ct);
    return response.Resource;
  }
}
