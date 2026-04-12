namespace Molino.Core.Services;

public interface ILlmService
{
  public Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken ct = default);
}
