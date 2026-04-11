using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.OAuth;
using Microsoft.VisualStudio.Services.WebApi;
using Molino.Core.Configs;
using Molino.Core.Models;
using Molino.Core.Services;

namespace Molino.Infrastructure.Services;

public sealed class AdoService(IOptions<AdoConfig> adoOptions, ILogger<AdoService> _logger) : IWorkItemService
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

    // Fetch comments
    var comments = new List<string>();
    try
    {
      var commentPages = await client.GetCommentsAsync(_config.Project, workItemId, cancellationToken: ct);
      comments.AddRange(commentPages.Comments.Select(c => c.Text));
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "Failed to fetch comments for WI#{Id}", workItemId);
    }

    // Process attachments - extract text from text-based attachments
    var attachments = new List<WorkItemAttachment>();
    if (workItem.Relations is not null)
    {
      foreach (var relation in workItem.Relations.Where(r => r.Rel == "AttachedFile"))
      {
        var attrs = relation.Attributes;
        var fileName = attrs.TryGetValue("name", out var n) ? n.ToString() ?? "" : "";
        var contentType = attrs.TryGetValue("contentType", out var c) ? c.ToString() ?? "" : "";

        string? extractedText = null;

        // Only extract text from known text-based formats
        if (contentType.StartsWith("text/") || fileName.EndsWith(".md") || fileName.EndsWith(".txt"))
        {
          // try
          // {
          //   using var stream = await client.GetAttachmentContentAsync()
          //   using var reader = new StreamReader(stream);
          //   extractedText = await reader.ReadToEndAsync(ct);
          // }
          // catch (Exception ex)
          // {
          //   _logger.LogWarning(ex, "Failed to read attachment {File}", fileName);
          // }
        }

        attachments.Add(new WorkItemAttachment
        {
          FileName = fileName,
          Url = relation.Url,
          ContentType = contentType,
          ExtractedText = extractedText
        });
      }
    }

    return new WorkItemDetails
    {
      Id = workItemId,
      Title = GetField<string>(fields, "System.Title") ?? "",
      Description = GetField<string>(fields, "System.Description") ?? "",
      AcceptanceCriteria = GetField<string>(fields, "Microsoft.VSTS.Common.AcceptanceCriteria") ?? "",
      AssignedTo = GetIdentityField(fields, "System.AssignedTo"),
      AreaPath = GetField<string>(fields, "System.AreaPath") ?? "",
      Comments = comments,
      Attachments = attachments
    };
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

  private static T? GetField<T>(IDictionary<string, object> fields, string key) =>
    fields.TryGetValue(key, out var value) && value is T typed ? typed : default;

  private static string GetIdentityField(IDictionary<string, object> fields, string key)
  {
    if (!fields.TryGetValue(key, out var value)) return "";
    // ADO returns identity as IdentityRef or string depending on context
    return value?.ToString() ?? "";
  }
}
