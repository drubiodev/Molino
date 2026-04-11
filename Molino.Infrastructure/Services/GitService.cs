using System.Diagnostics;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Molino.Core.Configs;
using Molino.Core.Models;
using Molino.Core.Services;

namespace Molino.Infrastructure.Services;

public class GitService(ILogger<GitService> _logger, IOptions<AdoConfig> _adoOptions) : IGitService
{
  public async Task<string> CloneAsync(string repoName, string targetDir, CancellationToken ct = default)
  {
    var ado = _adoOptions.Value;
    var token = await GetAccessTokenAsync(ado, ct);
    var cloneUrl = $"https://token:{token}@dev.azure.com/{new Uri(ado.OrganizationUrl).AbsolutePath.TrimStart('/')}/{ado.Project}/_git/{repoName}";

    _logger.LogInformation("Cloning repository {RepoName} into {TargetDir}", repoName, targetDir);

    await RunGitAsync("", ["clone", "--depth", "1", cloneUrl, targetDir], ct);
    return targetDir;
  }

  public Task CommitAllAsync(string repoDir, string message, CancellationToken ct = default)
  {
    throw new NotImplementedException();
  }

  public Task<string> CreateBranchAsync(string repoDir, string branchName, string baseBranch, CancellationToken ct = default)
  {
    throw new NotImplementedException();
  }

  public Task DeleteRemoteBranchAsync(string repoDir, string branchName, CancellationToken ct = default)
  {
    throw new NotImplementedException();
  }

  public Task<DiffStats> GetDiffStatsAsync(string repoDir, string baseBranch, CancellationToken ct = default)
  {
    throw new NotImplementedException();
  }

  public Task PushAsync(string repoDir, string branchName, CancellationToken ct = default)
  {
    throw new NotImplementedException();
  }

  private async Task<string> RunGitAsync(
    string workDir, string[] args, CancellationToken ct)
  {
    var psi = new ProcessStartInfo
    {
      FileName = "git",
      WorkingDirectory = string.IsNullOrEmpty(workDir) ? null : workDir,
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      UseShellExecute = false,
      CreateNoWindow = true
    };

    foreach (var arg in args)
      psi.ArgumentList.Add(arg);

    _logger.LogDebug("git {Args}", string.Join(" ", args));

    using var process = Process.Start(psi)
        ?? throw new InvalidOperationException("Failed to start git process");

    var stdout = await process.StandardOutput.ReadToEndAsync(ct);
    var stderr = await process.StandardError.ReadToEndAsync(ct);

    await process.WaitForExitAsync(ct);

    if (process.ExitCode != 0)
      throw new InvalidOperationException($"git {args[0]} failed (exit {process.ExitCode}): {stderr}");

    return stdout;
  }

  private static async Task<string> GetAccessTokenAsync(AdoConfig ado, CancellationToken ct)
  {
    var credential = new AzureCliCredential(new AzureCliCredentialOptions
    {
      TenantId = ado.TenantId
    });

    var token = await credential.GetTokenAsync(
      new Azure.Core.TokenRequestContext([$"{ado.ResourceId}/.default"]), ct);

    return token.Token;
  }
}
