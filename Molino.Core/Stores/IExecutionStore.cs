using Molino.Core.Models;

namespace Molino.Core.Stores;

public interface IExecutionStore
{
  Task<ExecutionRecord> CreateAsync(ExecutionRecord record, CancellationToken ct = default);
  Task<ExecutionRecord> UpdateAsync(ExecutionRecord record, CancellationToken ct = default);
  Task<ExecutionRecord?> UpdateStatusAsync(ImplementationRequest request, ExecutionStatus status, CancellationToken ct = default);
  Task<ExecutionRecord?> UpdateStepAsync(ImplementationRequest request, PipelineStep step, CancellationToken ct = default);
  Task<ExecutionRecord?> GetAsync(ImplementationRequest request, CancellationToken ct = default);
  Task<IReadOnlyList<ExecutionRecord>> GetByWorkItemAsync(int workItemId, CancellationToken ct = default);
}
