using AppStatistics.Contracts.TableSizes;

namespace AppStatistics.TableSizes;

public class TableSizeCatalog : ITableSizeCatalog
{
    private readonly Dictionary<string, Dictionary<int, RootTableSize>> _rootTableSizes = [];
    private readonly Dictionary<string, TableSize> _tableSizes = [];

    public TableSizeCatalog(IEnumerable<RootTableSize> rootTableSizes, IEnumerable<TableSize> tableSizes)
    {
        foreach (var tableSize in tableSizes)
            _tableSizes.TryAdd(tableSize.TableName, tableSize);

        foreach (var rootTableSize in rootTableSizes)
            if (_rootTableSizes.TryGetValue(rootTableSize.TableName, out var sizes))
                sizes.TryAdd(rootTableSize.RootId, rootTableSize);
            else
                _rootTableSizes.TryAdd(rootTableSize.TableName,
                    new Dictionary<int, RootTableSize>
                    {
                        { rootTableSize.RootId, rootTableSize }
                    });
    }

    public RootTableSize GetRootTableSize(int rootId, string tableName)
    {
        if (_rootTableSizes.TryGetValue(tableName, out var rootTableSize) &&
            rootTableSize.TryGetValue(rootId, out var size))
            return size;
        return new () { RootId = rootId, TableName = tableName, Records = ulong.MaxValue };
    }

    public TableSize GetTableSize(string tableName)
    {
        if (_tableSizes.TryGetValue(tableName, out var size))
            return size;
        
        return new() { TableName = tableName, Records = ulong.MaxValue };
    }
}