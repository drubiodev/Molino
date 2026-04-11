namespace Molino.Core.Models;

public sealed record class ImplementationRequest
{
  public required int WorkItemId { get; init; }
  public string? ExecutionId { get; init; }
}
