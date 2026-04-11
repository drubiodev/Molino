using System.Threading.Channels;
using Molino.Core.Models;
using Molino.Core.Pipeline;

namespace Molino.Api.BackgroundServices;

public sealed class PipelineWorker(ILogger<PipelineWorker> _logger, Channel<ImplementationRequest> _channel, IServiceScopeFactory _scopeFactory) : BackgroundService
{

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _logger.LogInformation("Pipeline worker started");
    await foreach (var request in _channel.Reader.ReadAllAsync(stoppingToken))
    {
      // for each pipeline run its own DI scope
      _ = Task.Run(async () =>
      {
        try
        {
          await using var scope = _scopeFactory.CreateAsyncScope();
          var orchestrator = scope.ServiceProvider.GetRequiredService<PipelineOrchestrator>();

          _logger.LogInformation("Processing WI#{WorkItemId}", request.WorkItemId);

          await orchestrator.ExecuteAsync(request, stoppingToken);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Unhandled error processing WI#{WorkItemId}", request.WorkItemId);
        }
      }, stoppingToken);
    }
  }
}
