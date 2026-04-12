using Molino.Core.Models;

namespace Molino.Core.Services;

public interface ICopilotService
{
  Task<ImplementationResult> ImplementAsync(string repoDir, string systemPrompt, string taskPrompt, CancellationToken ct = default);
}
