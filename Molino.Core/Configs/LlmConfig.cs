namespace Molino.Core.Configs;

public sealed class LlmConfig
{
  public string FoundryEndpoint { get; set; } = string.Empty;
  public string ModelId { get; set; } = string.Empty;
  public string CopilotCliUrl { get; set; } = "localhost:4321";
}
