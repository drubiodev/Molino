using Molino.Api.Models;

namespace Molino.Api.Endpoints;

public static class PipelineEndpoints
{
  public static void MapPipelineEndpoints(this WebApplication app)
  {
    var group = app.MapGroup("/api/pipeline").WithTags("Pipeline");

    // POST /api/pipeline
    group.MapPost("/", async (ImplementationRequest request, CancellationToken ct) =>
    {
      // store record
      // queue for processing
      // return 202 Accepted with location header for status endpoint
    });
  }
}
