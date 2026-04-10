namespace Molino.Api.Models;

public record class ImplementationRequest
{
  public required int WorkItemId { get; init; }
}
