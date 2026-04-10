using Molino.Core.Models;

namespace Molino.Infrastructure.Stores;

public interface IExecutionStore
{
  Task<ExecutionRecord> CreateAsync(ExecutionRecord record, CancellationToken ct = default);
  Task<ExecutionRecord> UpdateAsync(ExecutionRecord record, CancellationToken ct = default);
  Task<ExecutionRecord?> GetAsync(string id, CancellationToken ct = default);
  Task<IReadOnlyList<ExecutionRecord>> GetByWorkItemAsync(int workItemId, CancellationToken ct = default);
}

