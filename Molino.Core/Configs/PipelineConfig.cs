namespace Molino.Core.Configs;

public sealed class PipelineConfig
{
  public int JobTimeoutMinutes { get; set; } = 15;
  public int MaxRetries { get; set; } = 3;
  public string WorkDir { get; set; } = Path.Combine(Path.GetTempPath(), "molino");
  public Dictionary<string, string> Repos { get; set; } = [];
}
