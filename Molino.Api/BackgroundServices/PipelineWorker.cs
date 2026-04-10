using System.Threading.Channels;
using Molino.Core.Models;

namespace Molino.Api.BackgroundServices;

public sealed class PipelineWorker(ILogger<PipelineWorker> _logger, Channel<ImplementationRequest> _channel) : BackgroundService
{

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _logger.LogInformation("Pipeline worker started");
    await foreach (var request in _channel.Reader.ReadAllAsync(stoppingToken))
    {
      _logger.LogInformation("Processing work item {WorkItemId}", request.WorkItemId);
    }
  }
}
