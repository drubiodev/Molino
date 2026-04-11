using Molino.Core.Models;

namespace Molino.Core.Services;

public interface IWorkItemService
{
  Task<WorkItemDetails> GetWorkItemAsync(int workItemId, CancellationToken ct = default);
  Task CommentOnWorkItemAsync(int workItemId, string comment, CancellationToken ct = default);
  Task UpdateWorkItemStateAsync(int workItemId, string state, CancellationToken ct = default);
}
