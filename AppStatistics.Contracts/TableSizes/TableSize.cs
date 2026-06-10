using Encased.Contracts;

namespace AppStatistics.Contracts.TableSizes;

[Serializable]
public class TableSize : Uuid7Entity
{
    public string TableName { get; set; } = string.Empty;
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
    public ulong Records { get; set; }
}