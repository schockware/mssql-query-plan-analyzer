using Encased.Contracts;

namespace AppStatistics.Contracts.TableSizes;

[Serializable]
public class ArchivedTableSize : Uuid7Entity
{
    public ArchivedTableSize()
    {
    }

    public ArchivedTableSize(TableSize archiveFrom)
    {
        TableName = archiveFrom.TableName;
        TimeStamp = archiveFrom.TimeStamp;
        Records = archiveFrom.Records;
    }
    public string TableName { get; set; } = string.Empty;
    public DateTime ArchivedOn { get; set; } = DateTime.UtcNow;
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
    public ulong Records { get; set; }
}