using Azure.Identity;
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Options;
using Molino.Core.Configs;
using Molino.Core.Models;
using Molino.Core.Services;

namespace Molino.Infrastructure.Services;

public sealed class CopilotService(IOptions<LlmConfig> _config) : ICopilotService
{
  public async Task<ImplementationResult> ImplementAsync(string repoDir, string systemPrompt, string taskPrompt, CancellationToken ct = default)
  {
    string token = await GetToken(ct);

    await using var client = new CopilotClient(new CopilotClientOptions
    {
      CliUrl = "localhost:4321"
    });

    await using var session = await client.CreateSessionAsync(new SessionConfig
    {
      Model = _config.Value.ModelId,
      Provider = new ProviderConfig { }
    });
  }

  private async Task<string> GetToken(CancellationToken ct)
  {
    // Get MI token for Azure AI Foundry
    var credential = new DefaultAzureCredential();
    var tokenResult = await credential.GetTokenAsync(
        new Azure.Core.TokenRequestContext(
            ["https://cognitiveservices.azure.com/.default"]), ct);

    return tokenResult.Token;

  }
}
