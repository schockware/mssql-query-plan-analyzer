using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Encased.Access.MsSql;

public class EstimatedExecutionPlanAccessor(IOptions<Config> options)
{
    private const string Top25 =
        @"DECLARE @CSV varchar(max)

SELECT DISTINCT TOP 25 FocusedTableId
INTO #Focus
FROM FocusedTableExtension WITH(INDEX(IX_FocusedTableExtension_RootId))
WHERE RootId = @RootId
ORDER BY 1 DESC

SELECT @CSV = '[' + STRING_AGG(FocusedTableId, ',') + ']'
FROM #Focus";

    private const string EstimatePlan = "SET SHOWPLAN_XML ON;";
    private const string Query =
        @"SELECT ASDG.*, AG.FocusedTableExtensionId
FROM dbo.FocusedTableExtension as ASDG
INNER JOIN dbo.FocusedTable as AG
	ON AG.FocusedTableId = ASDG.FocusedTableId
WHERE ASDG.RootId = @RootId
AND FocusedTableId IN (
        SELECT
            [a0].[value]
        FROM
            OPENJSON('[]') WITH ([value] bigint '$') AS [a0]
    )
OPTION(RECOMPILE)";

    private const string TestQuery = "SELECT TOP 10 * FROM FocusedTableExtension";
    public async Task<string> GetEstimatedPlanForQuery(int RootId)
    {
        // var csv = await GetCsv(RootId);
        
        await using var conn = await ConnectionHelper.GetConnection(options.Value.ReportingConnectionString);
        await conn.OpenAsync();
        await using var estimate = conn.CreateCommand();
        estimate.CommandText = EstimatePlan; //You cannot parameterize a query once you do this.
        await estimate.ExecuteNonQueryAsync();
        
        await using var comm = conn.CreateCommand();
        comm.CommandText = Query.Replace("@RootId", RootId.ToString());
        // comm.Parameters.Add(new SqlParameter("@RootId", RootId));
        // comm.Parameters.Add(new SqlParameter("@CSV", csv));

        using var adapter = new SqlDataAdapter(comm);
        var ds = new DataSet();
        adapter.Fill(ds);
        

        var plan = ds.Tables[0]?.Rows[0]?.ItemArray[0]?.ToString() ?? string.Empty;
        return plan;
    }

    private Task<string> GetCsv(int RootId)
    {
        return Task.FromResult("[]");
        // await using var conn = await ConnectionHelper.GetConnection(options.Value.ReportingConnectionString);
        // await conn.OpenAsync();
        // await using var comm = new SqlCommand(Top25, conn);
        // comm.Parameters.Add(new SqlParameter("@RootId", RootId));
        // using var adapter = new SqlDataAdapter(comm);
        // var dt = new DataTable();
        // adapter.Fill(dt);
        // return dt.Rows.Count > 0 ? dt.Rows[0][0]?.ToString() ?? string.Empty : string.Empty;
    }

    public async Task<List<int>> GetRootIds()
    {
        await using var conn = await ConnectionHelper.GetConnection(options.Value.ReportingConnectionString);
        var roots = await conn.QueryAsync<Root>("SELECT RootId FROM RootTable WHERE DeletedUtc > GETUTCDATE()");
        return roots.Select(root => root.RootId).ToList();
    }
    public async Task<List<int>> GetXLRootIds()
    {
        await using var conn = await ConnectionHelper.GetConnection(options.Value.ReportingConnectionString);
        var roots = await conn.QueryAsync<Root>("SELECT R.RootId FROM RootTable as R INNER JOIN RootTableConfigs as RC ON RC.RootId = R.RootId WHERE RC.RootTableSize = 4");
        return roots.Select(root => root.RootId).ToList();
    }
    public class Root
    {
        public int RootId { get; set; }
    }
}