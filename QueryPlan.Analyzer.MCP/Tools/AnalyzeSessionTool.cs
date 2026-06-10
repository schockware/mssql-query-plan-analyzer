using System.ComponentModel;
using System.Text.Json;
using Encased.Access;
using ModelContextProtocol.Server;
using QueryPlan.Analyzer.Contracts;
using QueryPlan.Contracts;
using QueryPlan.Contracts.ShowPlanDotNet.Showplan.StatementBlocks;

namespace QueryPlan.Analyzer.MCP.Tools;

[McpServerToolType]
public class AnalyzeSessionTool(IDbAccess db)
{
    [McpServerTool]
    [Description("Runs query plan analysis for all plans in a session against the specified hypotheses. Pass an empty hypotheses array to run all available hypotheses.")]
    public async Task<string> AnalyzeSession(
        [Description("The session ID returned from get_sessions")]
        string sessionId,
        [Description("Hypothesis names to run. Available: BasicStatistics, SmallClientParameterSniffingIsRising, WillBreakForBiggestClient, RisingParameterSniffingIncreasesRatesForBadQueryPlans. Pass empty array to run all.")]
        string[] hypotheses)
    {
        if (!Guid.TryParse(sessionId, out var id))
            return JsonSerializer.Serialize(new { error = $"Invalid sessionId: {sessionId}" });

        var repo = db.GetRepository<QueryPlanRecord>();
        var records = (await repo.GetAsync(r => r.SampleSessionId == id)).ToList();

        var results = new List<IAnalysisResults>();
        var skipped = 0;

        foreach (var record in records)
        {
            if (!record.TryGetPlanXml(out var showPlan)
                || showPlan.BatchSequence is not { Length: > 0 }
                || showPlan.BatchSequence[0] is not { Length: > 0 }
                || showPlan.BatchSequence[0][0].Items is not { Length: > 0 }
                || showPlan.BatchSequence[0][0].Items[0] is not StmtSimple simple)
            {
                skipped++;
                continue;
            }

            var analysisResult = await new QueryPlanAnalyzer(record.Id, simple.QueryPlan).AnalyzeAsync();
            results.Add(analysisResult);
        }

        var selectedNames = hypotheses.Length > 0
            ? hypotheses
            : HypothesisRegistry.Available.Keys.ToArray();

        var hypothesisInstances = selectedNames
            .Where(HypothesisRegistry.Available.ContainsKey)
            .Select(name => HypothesisRegistry.Available[name]())
            .ToList();

        var unknownHypotheses = selectedNames
            .Where(name => !HypothesisRegistry.Available.ContainsKey(name))
            .ToList();

        foreach (var hypothesis in hypothesisInstances)
            await hypothesis.Hypothesize(results);

        var output = new
        {
            sessionId,
            totalPlansFound = records.Count,
            plansAnalyzed = results.Count,
            plansSkipped = skipped,
            unknownHypotheses,
            hypothesisResults = hypothesisInstances.Select(h => new
            {
                name = h.Name,
                description = h.Description,
                summary = h.Summarize(),
            }),
        };

        return JsonSerializer.Serialize(output, new JsonSerializerOptions { WriteIndented = true });
    }
}
