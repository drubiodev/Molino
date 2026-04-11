using System.Threading.Channels;
using Molino.Core.Models;
using Molino.Core.Stores;

namespace Molino.Api.Endpoints;

public static class PipelineEndpoints
{
  public static void MapPipelineEndpoints(this WebApplication app)
  {
    var group = app.MapGroup("/api/pipeline").WithTags("Pipeline");

    // POST /api/pipeline
    group.MapPost("/", async (ImplementationRequest request, Channel<ImplementationRequest> channel, IExecutionStore store, CancellationToken ct) =>
    {
      // store record
      var record = await store.CreateAsync(new ExecutionRecord
      {
        WorkItemId = request.WorkItemId,
        Status = ExecutionStatus.Queued,
        CurrentStep = PipelineStep.Queued
      }, ct);

      // queue for processing
      await channel.Writer.WriteAsync(request with { ExecutionId = record.Id }, ct);

      // return 202 Accepted with location header for status endpoint
      return Results.Accepted($"/api/pipeline/{record.Id}", new
      {
        record.Id,
        record.WorkItemId,
        record.Status,
        record.CurrentStep,
        message = "Pipeline queued for processing"
      });
    });
  }
}
