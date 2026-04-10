namespace Molino.Core.Configs;

public sealed class CosmosDbConfig
{
    public required string Account { get; set; }
    public required string Key { get; set; }
    public required string Database { get; set; }
    public required string Container { get; set; }
    public required string PartitionKeyPath { get; set; }
}
