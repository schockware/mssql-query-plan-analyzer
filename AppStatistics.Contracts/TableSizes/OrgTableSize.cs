using Encased.Contracts;

namespace AppStatistics.Contracts.TableSizes;

[Serializable]
public class RootTableSize : Uuid7Entity
{
    public int RootId { get; init; }
    public string TableName { get; init; } = string.Empty;
    public DateTime TimeStamp { get; init; } = DateTime.UtcNow;
    public ulong Records { get; init; }
    public string Note { get; init; } = string.Empty;
}