using System.Dynamic;
using Dapper;
using Encased.Access.MsSql.CommonQueries;
using Microsoft.Data.SqlClient;

namespace Encased.Access.MsSql;

public interface ICacheReader : IAsyncDisposable, IDisposable
{
    public int Index { get; }
    public int TotalRecords { get; }
    public string Source { get; }

    public bool IsDone { get; }
    public bool HasPendingRecords { get; }

    Task<List<CachedPlan.TextAndPlan>> NextAsync();
}

public sealed class CacheReader : ICacheReader
{
    public const int RecordsPerBatch = 100;
    public int Index { get; private set; }
    public int TotalRecords => Targets.Count;
    public string Source { get; }
    private List<CachedPlan.Handle> Targets { get; }
    private readonly SqlConnection _conn;
    public bool IsDone => Index >= TotalRecords;
    public bool HasPendingRecords => !IsDone;

    private CacheReader(SqlConnection conn, string source, List<CachedPlan.Handle> stats)
    {
        _conn = conn;
        Source = source;
        Targets = stats;
    }

    public static async Task<CacheReader> CreateAsync(string connectionString, string source, List<CachedPlan.Handle> stats)
    {
        var conn = await ConnectionHelper.GetConnection(connectionString);
        return new CacheReader(conn, source, stats);
    }

    public async Task<List<CachedPlan.TextAndPlan>> NextAsync()
    {
        if (IsDone)
            return [];
        var query = GetQuery();
        var parameters = GetParameters();
        try
        {
            var start = DateTime.Now;
            var results =
                (await _conn.QueryAsync<CachedPlan.TextAndPlan>(query, parameters)).ToList();
            Console.WriteLine($"{(DateTime.Now - start).TotalSeconds}s per {RecordsPerBatch}");

            Index += Math.Min(RecordsPerBatch, TotalRecords - Index + 1);
            return results;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Error fetching batch at index {Index}: {e.Message}");
            return [];
        }
    }

    public void Dispose()
    {
        _conn.Close();
        _conn.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _conn.CloseAsync();
        await _conn.DisposeAsync();
    }

    private static string GetQuery()
    {
        var inList = new List<string>();
        for (var i = 0; i < RecordsPerBatch; i++)
            inList.Add($"@handle_{i}");

        var handleParams = string.Join(",", inList);
        return CachedPlan.GetTextAndPlan.Replace("{handles}", handleParams);
    }

    private ExpandoObject GetParameters()
    {
        var parameters = new ExpandoObject();
        var next = Targets.Skip(Index).Take(RecordsPerBatch).ToList();

        for (var i = 0; i < RecordsPerBatch; i++)
            if(i < next.Count)
                parameters.TryAdd($"handle_{i}", next[i].PlanHandle);
            else
                parameters.TryAdd($"handle_{i}", (byte[])[]);

        return parameters;
    }
}
