using QueryPlan.Analyzer.Analyses.StatementAnalyses.PredicateAnalyses;
using QueryPlan.Analyzer.Contracts;

namespace QueryPlan.Analyzer.Hypotheses;

public class WillBreakForBiggestClient : IHypothesis
{
    public string Name => "WillBreakForBiggestClient";
    public string Description => "Identifies plans that only seek by RootId, which will break for BiggestClient roots that span multiple clients.";

    private readonly List<Guid> _matchingRecordIds = [];

    public Task Hypothesize(IAnalysisResults manifest)
    {
        if (IsTrue(manifest))
            _matchingRecordIds.Add(manifest.RecordId);
        return Task.CompletedTask;
    }

    public Task Hypothesize(IEnumerable<IAnalysisResults> manifests)
    {
        foreach (var manifest in manifests)
        {
            if (IsTrue(manifest))
                _matchingRecordIds.Add(manifest.RecordId);
        }
        return Task.CompletedTask;
    }

    public string Summarize()
    {
        var ids = string.Join(", ", _matchingRecordIds);
        return $"""
                Plans flagged as BiggestClient risk: {_matchingRecordIds.Count}
                Record IDs: {ids}
                """;
    }

    public static bool IsTrue(IAnalysisResults manifest)
        => manifest.SeekPredicateAnalyses.Any(a => a.CompletedSteps.Any(s => s is OnlyHasRootIdSeekPredicate
        {
            IsFound: true
        }));
}
