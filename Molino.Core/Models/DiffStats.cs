namespace Molino.Core.Models;

public sealed record DiffStats
{
  public required int LinesChanged { get; init; }
  public required int FilesChanged { get; init; }
  public required bool HasBinaryChanges { get; init; }
  public required string DiffSummary { get; init; }
}
