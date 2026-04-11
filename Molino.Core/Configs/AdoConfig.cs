namespace Molino.Core.Configs;

public sealed class AdoConfig
{
  public required string OrganizationUrl { get; set; }
  public required string TenantId { get; set; }
  public required string Project { get; set; }
  public required string ResourceId { get; set; }
}
