using Microsoft.Extensions.Logging;
using Molino.Core.Models;
using Molino.Core.Services;
using Molino.Core.Stores;

namespace Molino.Core.Pipeline;

public sealed class PipelineOrchestrator(ILogger<PipelineOrchestrator> _logger, IExecutionStore _store, IWorkItemService _workItems)
{
  public async Task ExecuteAsync(ImplementationRequest request, CancellationToken ct)
  {
    _logger.LogInformation("Starting pipeline for WI#{WorkItemId}", request.WorkItemId);

    var workDir = Path.Combine(Path.GetTempPath(), $"agent-{request.ExecutionId}");
    var branchName = $"agent/wi-{request.WorkItemId}-{request.ExecutionId?[..8]}";

    // Step1: Fetch work item details

    // Update record status
    await Task.WhenAll(
      _store.UpdateStatusAsync(request, ExecutionStatus.Running, ct),
      _store.UpdateStepAsync(request, PipelineStep.FetchingWorkItem, ct));

    _logger.LogInformation("Fetching work item details for WI#{WorkItemId}", request.WorkItemId);

    var workItem = await _workItems.GetWorkItemAsync(request.WorkItemId, ct);

    // Step2: Clone repo and checkout branch
    await _store.UpdateStepAsync(request, PipelineStep.CloningRepo, ct);
    // Step3: Generate a spec
  }
}
