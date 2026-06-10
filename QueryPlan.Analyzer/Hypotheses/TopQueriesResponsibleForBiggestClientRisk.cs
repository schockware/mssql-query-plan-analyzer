using QueryPlan.Analyzer.Analyses;
using QueryPlan.Analyzer.Analyses.StatementAnalyses.PredicateAnalyses;
using QueryPlan.Analyzer.Contracts;

namespace QueryPlan.Analyzer.Hypotheses;

public class TopQueriesResponsibleForBiggestClientRisk : IHypothesis
{
    public string Name => "TopQueriesResponsibleForBiggestClientRisk";
    public string Description => "Ranks tables and root IDs by how frequently they appear in BiggestClient-risky plans (root-only seek predicates).";

    private readonly Dictionary<string, int> _tableHitCounts = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<int, int> _rootHitCounts = [];
    private int _totalRisky;

    public Task Hypothesize(IAnalysisResults results)
    {
        var rootOnlySteps = results.SeekPredicateAnalyses
            .SelectMany(a => a.CompletedSteps)
            .OfType<OnlyHasRootIdSeekPredicate>()
            .Where(s => s.IsFound)
            .ToList();

        if (rootOnlySteps.Count == 0) return Task.CompletedTask;
        _totalRisky++;

        var sniffStep = results.CompletedPlanSteps
            .OfType<AnalyzeParameterSniffing>()
            .FirstOrDefault(s => s.RootId.HasValue);

        if (sniffStep?.RootId is { } rootId)
        {
            _rootHitCounts.TryAdd(rootId, 0);
            _rootHitCounts[rootId]++;
        }

        foreach (var tableName in rootOnlySteps.SelectMany(s => s.TableNames).Distinct())
        {
            _tableHitCounts.TryAdd(tableName, 0);
            _tableHitCounts[tableName]++;
        }

        return Task.CompletedTask;
    }

    public string Summarize()
    {
        if (_totalRisky == 0)
            return "No BiggestClient-risky plans detected.";

        var topTables = _tableHitCounts
            .OrderByDescending(kv => kv.Value)
            .Take(10)
            .Select(kv => $"{kv.Key}: {kv.Value} plans");

        var topRoots = _rootHitCounts
            .OrderByDescending(kv => kv.Value)
            .Take(10)
            .Select(kv => $"RootId={kv.Key}: {kv.Value} plans");

        return $"""
                Total BiggestClient-risky plans: {_totalRisky}
                Top tables with root-only seeks:
                {string.Join("\n", topTables)}
                Top roots compiling BiggestClient-risky plans:
                {string.Join("\n", topRoots)}
                """;
    }
}
