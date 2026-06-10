using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Encased.Access.MsSql;

public class RawQueryAccessor(IOptions<Config> options)
{
    private const string query = @"DECLARE @CSV varchar(max)
SELECT TOP 25 UniqueId INTO #Focus FROM UniqueTable WITH(INDEX(IX_UniqueTable_RootId_ArbitraryColumn_INCLUDES))
WHERE
    RootId = @RootId

SELECT
    @CSV = '[' + STRING_AGG(UniqueId, ',') + ']'
FROM
    #Focus  SET STATISTICS XML ON;  
SELECT UT.*, UTHT.HorizontalColumn1, UTHT.HorizontalColumn2, UTHT.HorizontalColumn3
INTO #Silent
FROM dbo.UniqueTable as UT
LEFT JOIN dbo.UniqueTableHorizontalTable as UTHT
	ON UTHT.UniqueId = UT.UniqueId
WHERE UT.RootId = @RootId
AND UTHT.DeletedUtc > GETUTCDATE()
AND UTHT.UniqueId IN (
        SELECT
            [a0].[value]
        FROM
            OPENJSON(@CSV) WITH ([value] bigint '$') AS [a0]
    ) 
OPTION(RECOMPILE)
SET
    STATISTICS XML OFF;";
    public async Task<string> GetActualPlanForQuery(int RootId)
    {
        await using var conn = await ConnectionHelper.GetConnection(options.Value.ReportingConnectionString);
        await conn.OpenAsync();
        await using var comm = new SqlCommand(query, conn);
        comm.Parameters.Add(new SqlParameter("@RootId", RootId));
        using var adapter = new SqlDataAdapter(comm);
        var dt = new DataTable();
        adapter.Fill(dt);
        if(dt.Rows.Count > 0)
            return dt.Rows[0][0].ToString();
        return string.Empty;
    }

    public async Task<List<int>> GetRootIds()
    {
        await using var conn = await ConnectionHelper.GetConnection(options.Value.ReportingConnectionString);
        var roots = await conn.QueryAsync<Root>("SELECT RootId FROM RootTable WHERE DeletedUtc > GETUTCDATE() AND FeatureFlags & 3 = 3");
        return roots.Select(root => root.RootId).ToList();
    }
    public async Task<List<int>> GetXLRootIds()
    {
        await using var conn = await ConnectionHelper.GetConnection(options.Value.ReportingConnectionString);
        var roots = await conn.QueryAsync<Root>("SELECT R.RootId FROM RootTable as R INNER JOIN RootTableConfigs as RTC ON RTC.RootId = R.RootId WHERE RTC.RootSize = 4");
        return roots.Select(root => root.RootId).ToList();
    }
    public class Root
    {
        public int RootId { get; set; }
    }
}