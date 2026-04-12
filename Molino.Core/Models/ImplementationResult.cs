namespace Molino.Core.Models;

public sealed record ImplementationResult
{
  public required bool Success { get; init; }
  public string? ErrorOutput { get; init; }
}