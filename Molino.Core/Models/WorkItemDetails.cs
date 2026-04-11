namespace Molino.Core.Models;

public sealed record WorkItemDetails
{
  public required int Id { get; init; }
  public required string Title { get; init; }
  public required string Description { get; init; }
  public required string AcceptanceCriteria { get; init; }
  public required string AssignedTo { get; init; }
  public required string AreaPath { get; init; }
  public IReadOnlyList<string> Comments { get; init; } = [];
  public IReadOnlyList<WorkItemAttachment> Attachments { get; init; } = [];
}

public sealed record WorkItemAttachment
{
  public required string FileName { get; init; }
  public required string Url { get; init; }
  public required string ContentType { get; init; }
  /// <summary>
  /// Text content extracted from the attachment, if applicable.
  /// Null for binary/unsupported attachments.
  /// </summary>
  public string? ExtractedText { get; init; }
}