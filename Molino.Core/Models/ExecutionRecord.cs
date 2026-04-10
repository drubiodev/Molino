namespace Molino.Core.Models;

public sealed record ExecutionRecord
{
  public string Id { get; init; } = Guid.NewGuid().ToString();
  public required int WorkItemId { get; init; }
  public required string RepoName { get; init; }
  public string? RequestedBy { get; init; }
  public string? BranchName { get; init; }
  public ExecutionStatus Status { get; init; } = ExecutionStatus.Queued;
  public PipelineStep CurrentStep { get; init; } = PipelineStep.Queued;
  public string? FailureReason { get; init; }
  public string? PullRequestUrl { get; init; }
  public int? DiffLinesChanged { get; init; }
  public int? DiffFilesChanged { get; init; }
  public int RetryCount { get; init; }
  public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
  public DateTimeOffset? CompletedAt { get; init; }
  public IReadOnlyList<StepLog> StepLogs { get; init; } = [];
}

public enum ExecutionStatus
{
  Queued,
  Running,
  Succeeded,
  Failed,
  Cancelled
}

public enum PipelineStep
{
  Queued,
  FetchingWorkItem,
  CloningRepo,
  Implementing,
  Validating,
  RetryingImplementation,
  CreatingPullRequest,
  CleaningUp,
  Completed
}

public sealed record StepLog
{
  public required PipelineStep Step { get; init; }
  public required StepOutcome Outcome { get; init; }
  public required DateTimeOffset StartedAt { get; init; }
  public required DateTimeOffset CompletedAt { get; init; }
  public string? Details { get; init; }
}

public enum StepOutcome
{
  Success,
  Failed,
  Skipped
}
