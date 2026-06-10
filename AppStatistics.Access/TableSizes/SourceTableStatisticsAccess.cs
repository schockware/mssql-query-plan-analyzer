using AppStatistics.Contracts.TableSizes;
using Dapper;
using Encased.Access.MsSql;
using Microsoft.Extensions.Options;

namespace AppStatistics.Access.TableSizes;

public class SourceTableStatisticsAccess(IOptions<Config> sourceOptions)
    : ITableStatisticsAccess
{
    public async Task<IEnumerable<TableSize>> GetAllTableSizes()
    {
        await using var conn = await ConnectionHelper.GetConnection(sourceOptions.Value.ReportingConnectionString);
        var sizes = await conn.QueryAsync<TableSize>(
            "SELECT TableName = o.NAME,\n  Records = i.rowcnt \nFROM sysindexes AS i\n  INNER JOIN sysobjects AS o ON i.id = o.id \nWHERE i.indid < 2  AND OBJECTPROPERTY(o.id, 'IsMSShipped') = 0\nAND xtype = 'U' ORDER BY o.NAME");
        return sizes;
    }

    public async Task<IEnumerable<RootTableSize>> GetAllRootTableSizes()
    {
        const string queryTemplate = "SELECT TableName = '{TableName}', RootId, Records = COUNT(*) FROM [{TableName}] GROUP BY RootId";
        var tableNames =  await GetApplicableTableNames();
        
        await using var conn = await ConnectionHelper.GetConnection(sourceOptions.Value.ReportingConnectionString);

        var sizes = new List<RootTableSize>();
        foreach (var tableName in tableNames)
        {
            try
            {
                var rootSize = await conn.QueryAsync<RootTableSize>(queryTemplate.Replace("{TableName}", tableName));
                sizes.AddRange(rootSize);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to get root table sizes for '{tableName}': {ex.Message}");
            }
        }

        return sizes;
    }

    private async Task<IEnumerable<string>> GetApplicableTableNames()
    {
        string query =
            $"SELECT DISTINCT TableName = COL.TABLE_NAME FROM INFORMATION_SCHEMA.COLUMNS as COL \tINNER JOIN INFORMATION_SCHEMA.TABLES AS TAB\n\t\tON TAB.TABLE_NAME = COL.TABLE_NAME WHERE COLUMN_NAME = 'RootId' AND TABLE_TYPE = 'BASE TABLE' AND TAB.TABLE_SCHEMA = 'dbo' AND COL.TABLE_NAME NOT IN ({string.Join(",", LargeTables.Names.Select(n => $"'{n}'"))})";
        
        await using var conn = await ConnectionHelper.GetConnection(sourceOptions.Value.ReportingConnectionString);
        var tables = await conn.QueryAsync<InformationSchemaTable>(query);
        return tables.Select(t => t.TableName);
    }

    private class InformationSchemaTable
    {
        public string TableName { get; init; } = string.Empty;
    }
}