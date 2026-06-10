using Dapper;
using Encased.Access.MsSql.CommonQueries;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Encased.Access.MsSql;

public class CacheAccessor(IOptions<Config> options)
{
    public async Task<ICacheReader> GetPrimaryReader()
    {
        await using var conn = await ConnectionHelper.GetConnection(options.Value.PrimaryConnectionString);
        conn.Open();
        var stats = (await GetHandles(conn)).ToList();
        Console.WriteLine($"Handles detected: {stats.Count}");
        return await CacheReader.CreateAsync(options.Value.PrimaryConnectionString, "Primary", stats);
    }
    public async Task<ICacheReader> GetReplicaReader()
    {
        await using var conn = await ConnectionHelper.GetConnection(options.Value.ReplicaConnectionString);
        conn.Open();
        var stats = (await GetHandles(conn)).ToList();
        Console.WriteLine($"Handles detected: {stats.Count}");
        return await CacheReader.CreateAsync(options.Value.ReplicaConnectionString, "Replica", stats);
    }
    public async Task<ICacheReader> GetReportingReader()
    {
        await using var conn = await ConnectionHelper.GetConnection(options.Value.ReportingConnectionString);
        await conn.OpenAsync();
        var stats = (await GetHandles(conn)).ToList();
        Console.WriteLine($"Handles detected: {stats.Count}");
        return await CacheReader.CreateAsync(options.Value.ReportingConnectionString, "Reporting", stats);
    }
    public async Task<IEnumerable<CachedPlan.TextAndPlan>> GetTextAndPlansPrimary(string likeClause)
    {
        await using var conn = await ConnectionHelper.GetConnection(options.Value.PrimaryConnectionString);
        await conn.OpenAsync();
        return await conn.QueryAsync<CachedPlan.TextAndPlan>(CachedPlan.GetTextAndPlanWithLike(likeClause));
    }
    public async Task<IEnumerable<CachedPlan.TextAndPlan>> GetTextAndPlansReplica(string likeClause)
    {
        await using var conn = await ConnectionHelper.GetConnection(options.Value.ReplicaConnectionString);
        await conn.OpenAsync();
        return await conn.QueryAsync<CachedPlan.TextAndPlan>(CachedPlan.GetTextAndPlanWithLike(likeClause));
    }
    private async Task<IEnumerable<CachedPlan.Handle>> GetHandles(SqlConnection conn)
        => await conn.QueryAsync<CachedPlan.Handle>(CachedPlan.GetHandles);
}
