using System.ComponentModel;
using System.Text.Json;
using Encased.Access;
using ModelContextProtocol.Server;
using QueryPlan.Contracts;

namespace QueryPlan.Analyzer.MCP.Tools;

[McpServerToolType]
public class GetSessionsTool(IDbAccess db)
{
    [McpServerTool]
    [Description("Returns all sample sessions with their timestamps. Use this to find a session by date before calling analyze_session.")]
    public async Task<string> GetSessions()
    {
        var repo = db.GetRepository<SampleSession>();
        var sessions = await repo.GetAsync(_ => true);

        var results = sessions
            .OrderByDescending(s => s.TimestampUtc)
            .Select(s => new
            {
                id = s.Id.ToString(),
                timestamp = s.Timestamp.ToString("o"),
                timestampUtc = s.TimestampUtc.ToString("o"),
            });

        return JsonSerializer.Serialize(results);
    }
}
