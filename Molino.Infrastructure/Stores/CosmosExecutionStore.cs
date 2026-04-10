using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Molino.Core.Configs;
using Molino.Core.Models;

namespace Molino.Infrastructure.Stores;

public sealed class CosmosExecutionStore : IExecutionStore
{
  private readonly CosmosClient _client;
  private readonly Container _container;
  private readonly ILogger<CosmosExecutionStore> _logger;

  public CosmosExecutionStore(IOptions<CosmosDbConfig> options, ILogger<CosmosExecutionStore> logger)
  {
    _logger = logger;
    var opts = options.Value;

    _client = new CosmosClient(opts.Account, opts.Key);
    _container = _client.GetContainer(opts.Database, opts.Container);
  }

  public async Task<ExecutionRecord> CreateAsync(ExecutionRecord record, CancellationToken ct = default)
  {
    var response = await _container.CreateItemAsync(record, new PartitionKey(record.WorkItemId.ToString()), cancellationToken: ct);
    return response.Resource;
  }

  public Task<ExecutionRecord?> GetAsync(string id, CancellationToken ct = default)
  {
    throw new NotImplementedException();
  }

  public Task<IReadOnlyList<ExecutionRecord>> GetByWorkItemAsync(int workItemId, CancellationToken ct = default)
  {
    throw new NotImplementedException();
  }

  public Task<ExecutionRecord> UpdateAsync(ExecutionRecord record, CancellationToken ct = default)
  {
    throw new NotImplementedException();
  }
}
