using QueryPlan.Analyzer.Contracts;

namespace QueryPlan.Analyzer.Hypotheses;

public class IndexSeekIsEffectivelyAFullScan(HighPercentageOfRowsReadFromIndex checker) : IHypothesis
{
    public string Name => "IndexSeekIsEffectivelyAFullScan";
    public string Description => "Flags plans where an index seek reads High or more of the table, making it effectively a full scan with seek overhead.";

    private readonly List<HighPercentageOfRowsReadFromIndex.Result> _findings = [];

    public async Task Hypothesize(IAnalysisResults results)
    {
        var candidates = await checker.Check(results);
        foreach (var result in candidates)
        {
            if (result.Skip) continue;
            if (result.RootThreshold >= HighPercentageOfRowsReadFromIndex.Threshold.High ||
                result.WholeTableThreshold >= HighPercentageOfRowsReadFromIndex.Threshold.High)
                _findings.Add(result);
        }
    }

    public string Summarize()
    {
        if (_findings.Count == 0)
            return "No plans found where index seeks are effectively full scans.";

        var byTable = _findings
            .GroupBy(f => f.TableName)
            .OrderByDescending(g => g.Count())
            .Select(g => $"{g.Key}: {g.Count()} plans (max root%: {g.Max(f => f.RootPercentage):P0})");

        return $"""
                Plans with index seek effectively a full scan: {_findings.Count}
                By table:
                {string.Join("\n", byTable)}
                """;
    }
}
