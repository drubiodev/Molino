using Azure.Identity;
using GitHub.Copilot.SDK;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Molino.Core.Configs;
using Molino.Core.Models;
using Molino.Core.Services;

namespace Molino.Infrastructure.Services;

public sealed class CopilotService(IOptions<LlmConfig> _config, ILogger<CopilotService> _logger) : ICopilotService
{
  public async Task<ImplementationResult> ImplementAsync(string repoDir, string systemPrompt, string taskPrompt, CancellationToken ct = default)
  {
    var cfg = _config.Value;
    var token = await GetToken(ct);
    var baseUrl = $"{cfg.FoundryEndpoint.TrimEnd('/')}/openai/v1/";

    _logger.LogInformation("Creating Copilot session against {BaseUrl} with model {Model}", baseUrl, cfg.ModelId);

    await using var client = new CopilotClient(new CopilotClientOptions
    {
      CliUrl = cfg.CopilotCliUrl
    });

    await using var session = await client.CreateSessionAsync(new SessionConfig
    {
      Model = cfg.ModelId,
      OnPermissionRequest = PermissionHandler.ApproveAll,
      Provider = new ProviderConfig
      {
        Type = "openai",
        BaseUrl = baseUrl,
        BearerToken = token,
        WireApi = "responses",
      }
    });

    // Send system prompt to establish agent context
    var fullSystemPrompt = $"You are working in the directory: {repoDir}\n\n{systemPrompt}";
    await session.SendAsync(new MessageOptions { Prompt = fullSystemPrompt });

    // Send task prompt and await the implementation response
    _logger.LogInformation("Sending task prompt ({Length} chars) to Copilot session", taskPrompt.Length);
    var response = await session.SendAndWaitAsync(
        new MessageOptions { Prompt = taskPrompt },
        timeout: TimeSpan.FromMinutes(10),
        cancellationToken: ct);

    if (response?.Data?.Content is { Length: > 0 } content)
    {
      _logger.LogInformation("Copilot session completed successfully ({ResponseLen} chars)", content.Length);
      return new ImplementationResult { Success = true };
    }

    _logger.LogWarning("Copilot session returned empty response");
    return new ImplementationResult
    {
      Success = false,
      ErrorOutput = "Copilot session returned an empty response"
    };
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
