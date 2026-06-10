using AppStatistics.Contracts.TableSizes;

namespace AppStatistics.Access.TableSizes;

public interface ITableStatisticsAccess
{
    Task<IEnumerable<RootTableSize>> GetAllRootTableSizes();
    Task<IEnumerable<TableSize>> GetAllTableSizes();
}