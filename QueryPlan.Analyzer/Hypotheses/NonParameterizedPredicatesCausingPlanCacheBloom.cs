using QueryPlan.Analyzer.Analyses;
using QueryPlan.Analyzer.Analyses.StatementAnalyses.PredicateAnalyses;
using QueryPlan.Analyzer.Contracts;

namespace QueryPlan.Analyzer.Hypotheses;

public class NonParameterizedPredicatesCausingPlanCacheBloom : IHypothesis
{
    public string Name => "NonParameterizedPredicatesCausingPlanCacheBloom";
    public string Description => "Groups plans by non-parameterized column sets to identify which columns are causing plan cache bloat.";

    private readonly Dictionary<string, int> _columnPatternCounts = new(StringComparer.OrdinalIgnoreCase);
    private int _totalAffected;

    public Task Hypothesize(IAnalysisResults results)
    {
        var isOurs = results.CompletedPlanSteps.Any(s => s is IsOurQuery { IsFound: true });
        if (!isOurs) return Task.CompletedTask;

        var step = results.PredicateAnalyses
            .SelectMany(a => a.CompletedSteps)
            .OfType<HasNonParameterizedPredicate>()
            .FirstOrDefault(s => s.IsFound);

        if (step == null) return Task.CompletedTask;

        _totalAffected++;
        var key = string.Join(",", step.ColumnNames.OrderBy(c => c));
        _columnPatternCounts.TryAdd(key, 0);
        _columnPatternCounts[key]++;

        return Task.CompletedTask;
    }

    public string Summarize()
    {
        if (_totalAffected == 0)
            return "No non-parameterized predicate plans detected.";

        var topPatterns = _columnPatternCounts
            .OrderByDescending(kv => kv.Value)
            .Take(10)
            .Select(kv => $"[{kv.Key}]: {kv.Value} plans");

        return $"""
                Plans with non-parameterized predicates: {_totalAffected}
                Distinct column patterns: {_columnPatternCounts.Count}
                Top patterns causing bloom:
                {string.Join("\n", topPatterns)}
                """;
    }
}
