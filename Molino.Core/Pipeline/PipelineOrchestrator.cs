using Microsoft.Extensions.Logging;
using Molino.Core.Models;
using Molino.Core.Stores;

namespace Molino.Core.Pipeline;

public sealed class PipelineOrchestrator(ILogger<PipelineOrchestrator> _logger, IExecutionStore _store)
{
  public async Task ExecuteAsync(ImplementationRequest request, CancellationToken ct)
  {
    _logger.LogInformation("Starting pipeline for WI#{WorkItemId}", request.WorkItemId);

    // Update record status
    var record = await _store.GetAsync(request, ct);

    if (record is not null)
    {
      await _store.UpdateAsync(record with
      {
        Status = ExecutionStatus.Running,
        CurrentStep = PipelineStep.FetchingWorkItem
      }, ct);
    }
  }
}
