using Molino.Core.Models;

namespace Molino.Core.Services;

public interface ISpecService
{
  Task<SpecResult> GenerateAsync(WorkItemDetails workItem, string workSpaceDir, CancellationToken ct = default);
}
