using Molino.Core.Models;

namespace Molino.Core.Services;

public interface IGitService
{
  Task<string> CloneAsync(string repoName, string targetDir, CancellationToken ct = default);
  Task<string> CreateBranchAsync(string repoDir, string branchName, string baseBranch, CancellationToken ct = default);
  Task CommitAllAsync(string repoDir, string message, CancellationToken ct = default);
  Task PushAsync(string repoDir, string branchName, CancellationToken ct = default);
  Task DeleteRemoteBranchAsync(string repoDir, string branchName, CancellationToken ct = default);
  Task<DiffStats> GetDiffStatsAsync(string repoDir, string baseBranch, CancellationToken ct = default);
}

