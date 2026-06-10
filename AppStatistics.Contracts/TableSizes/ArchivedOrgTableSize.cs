using Encased.Contracts;

namespace AppStatistics.Contracts.TableSizes;

[Serializable]
public class ArchivedRootTableSize : Uuid7Entity
{
    public ArchivedRootTableSize()
    {
    }

    public ArchivedRootTableSize(RootTableSize archiveFrom)
    {
        RootId = archiveFrom.RootId;
        TableName = archiveFrom.TableName;
        TimeStamp = archiveFrom.TimeStamp;
        Records = archiveFrom.Records;
    }

    public int RootId { get; init; }
    public string TableName { get; init; } = string.Empty;
    public DateTime ArchivedOn { get; init; } = DateTime.UtcNow;
    public DateTime TimeStamp { get; init; } = DateTime.UtcNow;
    public ulong Records { get; init; }
}