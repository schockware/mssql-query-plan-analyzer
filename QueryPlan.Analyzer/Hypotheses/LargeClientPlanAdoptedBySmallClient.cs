using AppStatistics.Contracts.TableSizes;
using QueryPlan.Analyzer.Analyses;
using QueryPlan.Analyzer.Analyses.StatementAnalyses.PredicateAnalyses;
using QueryPlan.Analyzer.Contracts;

namespace QueryPlan.Analyzer.Hypotheses;

public class LargeClientPlanAdoptedBySmallClient(ITableSizeCatalog catalog) : IHypothesis
{
    public string Name => "LargeClientPlanAdoptedBySmallClient";
    public string Description => "Detects parameter-sniffed plans compiled by small clients that will perform poorly when reused by large clients.";

    // Roots whose data represents less than this fraction of the total table are considered small clients
    private const float SmallClientThreshold = 0.05f;

    private int _totalSniffed;
    private readonly List<(int RootId, string TableName, float RootFraction)> _smallClientCompilations = [];

    public Task Hypothesize(IAnalysisResults results)
    {
        var sniffStep = results.CompletedPlanSteps
            .OfType<AnalyzeParameterSniffing>()
            .FirstOrDefault(s => s.IsParameterSniffed && s.RootId.HasValue);

        if (sniffStep == null) return Task.CompletedTask;
        _totalSniffed++;

        var rootId = sniffStep.RootId!.Value;

        var tableNames = results.SeekPredicateAnalyses
            .SelectMany(a => a.CompletedSteps)
            .OfType<OnlyHasRootIdSeekPredicate>()
            .Where(s => s.IsFound)
            .SelectMany(s => s.TableNames)
            .Distinct();

        foreach (var tableName in tableNames)
        {
            var tableSize = catalog.GetTableSize(tableName);
            if (tableSize.Records == 0) continue;

            var rootSize = catalog.GetRootTableSize(rootId, tableName);
            var fraction = (float)rootSize.Records / tableSize.Records;

            if (fraction < SmallClientThreshold)
                _smallClientCompilations.Add((rootId, tableName, fraction));
        }

        return Task.CompletedTask;
    }

    public string Summarize()
    {
        if (_smallClientCompilations.Count == 0)
            return "No small-client compiled plans detected.";

        var topRoots = _smallClientCompilations
            .GroupBy(c => c.RootId)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .Select(g => $"RootId={g.Key}: {g.Count()} tables (avg {g.Average(c => c.RootFraction):P1} of table)");

        return $"""
                Total parameter-sniffed plans: {_totalSniffed}
                Plans compiled by small clients (<{SmallClientThreshold:P0} of table): {_smallClientCompilations.Count}
                Top small-client compilers:
                {string.Join("\n", topRoots)}
                """;
    }
}
