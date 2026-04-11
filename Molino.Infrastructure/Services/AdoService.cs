using Azure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.OAuth;
using Microsoft.VisualStudio.Services.WebApi;
using Molino.Core.Configs;
using Molino.Core.Models;
using Molino.Core.Services;

namespace Molino.Infrastructure.Services;

public sealed class AdoService(IOptions<AdoConfig> adoOptions) : IWorkItemService
{
  private readonly AdoConfig _config = adoOptions.Value;
  public Task CommentOnWorkItemAsync(int workItemId, string comment, CancellationToken ct = default)
  {
    throw new NotImplementedException();
  }

  public async Task<WorkItemDetails> GetWorkItemAsync(int workItemId, CancellationToken ct = default)
  {
    var client = await GetClientAsync(ct);

    var workItem = await client.GetWorkItemAsync(
      _config.Project, workItemId,
      expand: WorkItemExpand.All,
      cancellationToken: ct);

    var fields = workItem.Fields;

    throw new NotImplementedException();
  }

  public Task UpdateWorkItemStateAsync(int workItemId, string state, CancellationToken ct = default)
  {
    throw new NotImplementedException();
  }

  private async Task<WorkItemTrackingHttpClient> GetClientAsync(CancellationToken ct)
  {
    // TODO: using CLI for local dev but need to switch to Managed Identity for prod
    var credential = new AzureCliCredential(new AzureCliCredentialOptions
    {
      TenantId = _config.TenantId
    });

    var token = await credential.GetTokenAsync(new Azure.Core.TokenRequestContext([$"{_config.ResourceId}/.default"]), ct);

    var credentials = new VssOAuthAccessTokenCredential(token.Token);

    var connection = new VssConnection(new Uri(_config.OrganizationUrl), credentials);
    return await connection.GetClientAsync<WorkItemTrackingHttpClient>(ct);
  }
}
