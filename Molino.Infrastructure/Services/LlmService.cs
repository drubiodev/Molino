using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Options;
using Molino.Core.Configs;
using Molino.Core.Services;

namespace Molino.Infrastructure.Services;

public sealed class LlmService : ILlmService
{
  private readonly AIProjectClient _client;
  private readonly string _model;

  public LlmService(IOptions<LlmConfig> config)
  {
    _client = new AIProjectClient(new Uri(config.Value.FoundryEndpoint), new DefaultAzureCredential());
    _model = config.Value.ModelId;
  }

  public async Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken ct = default)
  {
    ChatClientAgent agent = _client.AsAIAgent(model: _model, instructions: systemPrompt);
    var response = await agent.RunAsync(userPrompt, cancellationToken: ct);
    return response.Text ?? string.Empty;
  }
}
