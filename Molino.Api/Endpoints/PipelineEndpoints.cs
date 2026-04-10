using Molino.Core.Models;
using Molino.Infrastructure.Stores;

namespace Molino.Api.Endpoints;

public static class PipelineEndpoints
{
  public static void MapPipelineEndpoints(this WebApplication app)
  {
    var group = app.MapGroup("/api/pipeline").WithTags("Pipeline");

    // POST /api/pipeline
    group.MapPost("/", async (ImplementationRequest request, IExecutionStore store, CancellationToken ct) =>
    {
      // store record
      var record = await store.CreateAsync(new ExecutionRecord
      {
        WorkItemId = request.WorkItemId,
        Status = ExecutionStatus.Queued,
        CurrentStep = PipelineStep.Queued
      }, ct);
      // queue for processing
      // return 202 Accepted with location header for status endpoint
    });
  }
}
