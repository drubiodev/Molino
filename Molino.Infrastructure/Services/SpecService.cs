using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using Molino.Core.Models;
using Molino.Core.Services;

namespace Molino.Infrastructure.Services;

public sealed class SpecService(ILlmService _llm, ILogger<SpecService> _logger) : ISpecService
{
  private static readonly string SpecPrompt = LoadEmbeddedPrompt("SpecPrompt.md");
  private static readonly string PlanPrompt = LoadEmbeddedPrompt("PlanPrompt.md");
  private static readonly string TasksPrompt = LoadEmbeddedPrompt("TasksPrompt.md");

  public async Task<SpecResult> GenerateAsync(WorkItemDetails workItem, string workSpaceDir, CancellationToken ct = default)
  {
    // Step 1: Generate the specification from the work item
    _logger.LogInformation("Generating spec for WI#{WorkItemId}: {Title}", workItem.Id, workItem.Title);

    var specUserPrompt = BuildSpecUserPrompt(workItem);
    var specContent = await _llm.CompleteAsync(SpecPrompt, specUserPrompt, ct);

    // Step 2: Generate the implementation plan from the spec + repo context
    _logger.LogInformation("Generating plan for WI#{WorkItemId}", workItem.Id);

    var repoStructure = ScanDirectory(workSpaceDir, maxDepth: 3);
    var planUserPrompt = BuildPlanUserPrompt(workItem, specContent, repoStructure);
    var planContent = await _llm.CompleteAsync(PlanPrompt, planUserPrompt, ct);

    // Step 3: Generate the task breakdown from the spec + plan
    _logger.LogInformation("Generating tasks for WI#{WorkItemId}", workItem.Id);

    var tasksUserPrompt = BuildTasksUserPrompt(specContent, planContent);
    var tasksContent = await _llm.CompleteAsync(TasksPrompt, tasksUserPrompt, ct);

    return new SpecResult
    {
      SpecContent = specContent,
      PlanContent = planContent,
      TasksContent = tasksContent
    };
  }

  private static string BuildSpecUserPrompt(WorkItemDetails workItem)
  {
    var sb = new StringBuilder();
    sb.AppendLine($"## Work Item #{workItem.Id}");
    sb.AppendLine();
    sb.AppendLine($"**Title**: {workItem.Title}");
    sb.AppendLine();
    sb.AppendLine($"**Area Path**: {workItem.AreaPath}");
    sb.AppendLine();
    sb.AppendLine("### Description");
    sb.AppendLine(workItem.Description);
    sb.AppendLine();
    sb.AppendLine("### Acceptance Criteria");
    sb.AppendLine(workItem.AcceptanceCriteria);

    if (workItem.Comments.Count > 0)
    {
      sb.AppendLine();
      sb.AppendLine("### Discussion / Comments");
      foreach (var comment in workItem.Comments)
      {
        sb.AppendLine($"- {comment}");
      }
    }

    if (workItem.Attachments.Count > 0)
    {
      var textAttachments = workItem.Attachments
        .Where(a => a.ExtractedText is not null)
        .ToList();

      if (textAttachments.Count > 0)
      {
        sb.AppendLine();
        sb.AppendLine("### Attachment Content");
        foreach (var attachment in textAttachments)
        {
          sb.AppendLine($"#### {attachment.FileName}");
          sb.AppendLine(attachment.ExtractedText);
          sb.AppendLine();
        }
      }
    }

    return sb.ToString();
  }

  private static string BuildPlanUserPrompt(WorkItemDetails workItem, string specContent, string repoStructure)
  {
    var sb = new StringBuilder();
    sb.AppendLine("## Feature Specification");
    sb.AppendLine();
    sb.AppendLine(specContent);
    sb.AppendLine();
    sb.AppendLine("## Repository Structure");
    sb.AppendLine();
    sb.AppendLine("```");
    sb.AppendLine(repoStructure);
    sb.AppendLine("```");
    sb.AppendLine();
    sb.AppendLine($"Work Item #{workItem.Id}: {workItem.Title}");
    return sb.ToString();
  }

  private static string BuildTasksUserPrompt(string specContent, string planContent)
  {
    var sb = new StringBuilder();
    sb.AppendLine("## Feature Specification (spec.md)");
    sb.AppendLine();
    sb.AppendLine(specContent);
    sb.AppendLine();
    sb.AppendLine("## Implementation Plan (plan.md)");
    sb.AppendLine();
    sb.AppendLine(planContent);
    return sb.ToString();
  }

  private static string ScanDirectory(string rootDir, int maxDepth)
  {
    var sb = new StringBuilder();
    ScanDirectoryRecursive(sb, rootDir, "", 0, maxDepth);
    return sb.ToString();
  }

  private static void ScanDirectoryRecursive(StringBuilder sb, string dir, string indent, int depth, int maxDepth)
  {
    if (depth >= maxDepth || !Directory.Exists(dir))
      return;

    var dirName = Path.GetFileName(dir);
    if (dirName is ".git" or "bin" or "obj" or "node_modules" or ".vs")
      return;

    sb.AppendLine($"{indent}{dirName}/");
    var nextIndent = indent + "  ";

    foreach (var file in Directory.EnumerateFiles(dir))
    {
      sb.AppendLine($"{nextIndent}{Path.GetFileName(file)}");
    }

    foreach (var subDir in Directory.EnumerateDirectories(dir))
    {
      ScanDirectoryRecursive(sb, subDir, nextIndent, depth + 1, maxDepth);
    }
  }

  private static string LoadEmbeddedPrompt(string fileName)
  {
    var assembly = typeof(SpecService).Assembly;

    // The Core assembly embeds the prompts
    var coreAssembly = Assembly.Load("Molino.Core");
    var resourceName = coreAssembly.GetManifestResourceNames()
      .FirstOrDefault(n => n.EndsWith(fileName, StringComparison.OrdinalIgnoreCase))
      ?? throw new InvalidOperationException($"Embedded prompt '{fileName}' not found. Available: {string.Join(", ", coreAssembly.GetManifestResourceNames())}");

    using var stream = coreAssembly.GetManifestResourceStream(resourceName)!;
    using var reader = new StreamReader(stream);
    return reader.ReadToEnd();
  }
}
