namespace AppStatistics.Contracts.TableSizes;

public interface ITableSizeCatalog
{
    RootTableSize GetRootTableSize(int rootId, string tableName);
    TableSize GetTableSize(string tableName);
}