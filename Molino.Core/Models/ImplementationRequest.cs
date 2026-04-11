namespace Molino.Core.Models;

public record class ImplementationRequest
{
  public required int WorkItemId { get; init; }
  public string? ExecutionId { get; init; }
}
