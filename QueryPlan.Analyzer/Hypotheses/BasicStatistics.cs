using QueryPlan.Analyzer.Analyses;
using QueryPlan.Analyzer.Analyses.StatementAnalyses.PredicateAnalyses;
using QueryPlan.Analyzer.Contracts;

namespace QueryPlan.Analyzer.Hypotheses;

public class BasicStatistics : IHypothesis
{
    public string Name => "BasicStatistics";
    public string Description => "Counts parameter sniffed plans, root-only seek predicate plans, and non-parameterized predicate plans across the session.";
    public int TotalParameterSniffed { get; private set; }
    public int TotalRootOnlyPredicates { get; private set; }
    public int TotalNonParameterizedPredicates { get; private set; }

    private readonly List<IAnalysisResults> _rootOnlyPredicateResults = [];
    private readonly List<IAnalysisResults> _nonParameterizedPredicateResults = [];

    public async Task Hypothesize(IAnalysisResults results)
    {
        await CountCompiledRoots(results);
        await CountRootOnlySeekPredicates(results);
        await CountHasNonParameterizedPredicates(results);
    }

    private Task CountCompiledRoots(IAnalysisResults results)
    {
        var step =
            results.CompletedPlanSteps.FirstOrDefault(step => step is AnalyzeParameterSniffing
                {
                    IsParameterSniffed: true
                }) as AnalyzeParameterSniffing;
        if (step == null)
            return Task.CompletedTask;

        TotalParameterSniffed++;
        return Task.CompletedTask;
    }

    private Task CountRootOnlySeekPredicates(IAnalysisResults results)
    {
        var step = results.SeekPredicateAnalyses.SelectMany(a => a.CompletedSteps)
            .FirstOrDefault(step => step is OnlyHasRootIdSeekPredicate
            {
                IsFound: true
            });
        if (step == null)
            return Task.CompletedTask;

        _rootOnlyPredicateResults.Add(results);
        TotalRootOnlyPredicates++;
        return Task.CompletedTask;
    }

    private Task CountHasNonParameterizedPredicates(IAnalysisResults results)
    {
        var isOurQuery = results.CompletedPlanSteps.Any(step => step is IsOurQuery { IsFound: true });
        if (!isOurQuery)
            return Task.CompletedTask;

        var step = results.PredicateAnalyses.SelectMany(a => a.CompletedSteps)
            .FirstOrDefault(step => step is HasNonParameterizedPredicate
            {
                IsFound: true
            });
        if (step == null)
            return Task.CompletedTask;

        _nonParameterizedPredicateResults.Add(results);
        TotalNonParameterizedPredicates++;
        return Task.CompletedTask;
    }

    public void DisplayNonParameterizedPredicates()
    {
        foreach (var results in _nonParameterizedPredicateResults)
        {
            var step = results.PredicateAnalyses.SelectMany(a => a.CompletedSteps)
                .FirstOrDefault(step => step is HasNonParameterizedPredicate
                {
                    IsFound: true
                }) as HasNonParameterizedPredicate;
            Console.WriteLine($"{results.RecordId}");
            foreach (var name in step?.ColumnNames ?? [])
                Console.WriteLine($"--{name}");
        }
    }

    public void DisplayNonParameterizedColumns()
    {
        foreach (var results in _nonParameterizedPredicateResults)
        {
            var step = results.PredicateAnalyses.SelectMany(a => a.CompletedSteps)
                .FirstOrDefault(step => step is HasNonParameterizedPredicate
                {
                    IsFound: true
                }) as HasNonParameterizedPredicate;
            foreach (var name in step?.ColumnNames ?? [])
                Console.WriteLine($"--{name}");
        }
    }

    public string Summarize() =>
        $"""
         Total parameter sniffed plans: {TotalParameterSniffed}
         Total root-only seek predicate plans: {TotalRootOnlyPredicates}
         Total non-parameterized predicate plans: {TotalNonParameterizedPredicates}
         """;
}
