using AppStatistics.Contracts.TableSizes;

namespace AppStatistics.Access.TableSizes;

public class TieredTableStatisticsAccess(CachedTableStatisticsAccess cached, SourceTableStatisticsAccess source)
: ITableStatisticsAccess
{
    private static DateTime LastMonth => DateTime.UtcNow.AddMonths(-1);
    
    public async Task<IEnumerable<RootTableSize>> GetAllRootTableSizes()
    {
        var cachedSizes = (await cached.GetAllRootTableSizes()).ToList();
        if (cachedSizes.Count == 0 || AreAnyStale(cachedSizes))
            return await cached.Refresh(source.GetAllRootTableSizes);

        return cachedSizes;
    }

    public async Task<IEnumerable<TableSize>> GetAllTableSizes()
    {
        var cachedSizes = (await cached.GetAllTableSizes()).ToList();
        if (cachedSizes.Count == 0 ||AreAnyStale(cachedSizes))
            return await cached.Refresh(source.GetAllTableSizes);

        return cachedSizes;
    }
    
    private static bool AreAnyStale(IEnumerable<RootTableSize> sizes)
        => sizes.Any(s => s.TimeStamp <= LastMonth);
    private static bool AreAnyStale(IEnumerable<TableSize> sizes)
        => sizes.Any(s => s.TimeStamp <= LastMonth);
}