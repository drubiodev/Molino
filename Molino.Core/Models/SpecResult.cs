namespace Molino.Core.Models;

public sealed record SpecResult
{
  public required string SpecContent { get; init; }
  public required string PlanContent { get; init; }
  public required string TasksContent { get; init; }
}
