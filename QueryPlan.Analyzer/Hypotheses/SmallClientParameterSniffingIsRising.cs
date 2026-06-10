using System.Collections.Concurrent;
using QueryPlan.Analyzer.Analyses;
using QueryPlan.Analyzer.Contracts;

namespace QueryPlan.Analyzer.Hypotheses;

public class SmallClientParameterSniffingIsRising : IHypothesis
{
    public string Name => "SmallClientParameterSniffingIsRising";
    public string Description => "Tracks which root IDs are compiled into query plans to identify small-client parameter sniffing concentration.";

    public ConcurrentDictionary<int, int> CompiledRootQuantity { get; } = [];
    public int UnknownCompiledRootQuantity { get; private set; }
    public int TotalParameterSniffed { get; private set; }

    public async Task Hypothesize(IAnalysisResults results)
    {
        await CountCompiledRoots(results);
    }

    private Task CountCompiledRoots(IAnalysisResults results)
    {
        var step = results.CompletedPlanSteps.FirstOrDefault(step => step is AnalyzeParameterSniffing) as AnalyzeParameterSniffing;
        if (step == null)
            return Task.CompletedTask;

        TotalParameterSniffed++;
        if (step.RootId != null)
            CompiledRootQuantity.AddOrUpdate(step.RootId.Value, _ => 1, (_, original) => original + 1);
        else
            UnknownCompiledRootQuantity++;

        return Task.CompletedTask;
    }

    public string Summarize()
    {
        var topRoots = CompiledRootQuantity
            .OrderByDescending(kv => kv.Value)
            .Take(10)
            .Select(kv => $"{kv.Key}={kv.Value}");

        return $"""
                Total parameter sniffed: {TotalParameterSniffed}
                Unknown compiled root: {UnknownCompiledRootQuantity}
                Top roots by compiled plan count: {string.Join(", ", topRoots)}
                """;
    }
}
