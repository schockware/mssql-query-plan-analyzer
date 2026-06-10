using QueryPlan.Analyzer.Analyses.StatementAnalyses.PredicateAnalyses;
using QueryPlan.Analyzer.Contracts;
using QueryPlan.Analyzer.Contracts.Analyses;
using QueryPlan.Contracts.ShowPlanDotNet.Showplan;

namespace QueryPlan.Analyzer.Analyses;

public class AnalyzeParameterSniffing : IQueryPlanAnalyzerStep
{
    public IQueryPlanAnalyzerStep.AnalysisStatus Status { get; private set; }
    public bool IsParameterSniffed { get; private set; }
    public string RootIdParameterName { get; private set; } = "@__rootId";
    public int? RootId { get; private set; }
    public Dictionary<string, string> Parameters { get; } = [];

    public void Run(QueryPlanNode plan)
    {
        if (plan?.ParameterList == null)
            return;

        IsParameterSniffed = plan.ParameterList.Length > 0;
        foreach (var param in plan.ParameterList)
        {
            var sanitizedColumn = SanitizeParameterName(param.Column);
            if (!RootId.HasValue && sanitizedColumn.StartsWith(RootIdParameterName)
                                && !param.ParameterCompiledValue.Contains('?'))
                RootId = Parse(param.ParameterCompiledValue);

            Parameters.Add(sanitizedColumn, param.ParameterCompiledValue);
        }

        if (IsParameterSniffed && !RootId.HasValue)
            Status = IQueryPlanAnalyzerStep.AnalysisStatus.HasDependencies;
        else
            Status = IQueryPlanAnalyzerStep.AnalysisStatus.Completed;
    }

    public async Task HandleDependencies(IAnalysisResults results)
    {
        if (RootId.HasValue)
            return;

        var source = new CancellationTokenSource();

        var tasks = new List<Task>();
        tasks.AddRange(results.SeekPredicateAnalyses.Select(p => HasGetRootStep(p, source)));
        tasks.AddRange(results.PredicateAnalyses.Select(p => HasGetRootStep(p, source)));

        await Task.WhenAll(tasks);
    }

    private async Task HasGetRootStep(SeekPredicateAnalysis analysis, CancellationTokenSource source)
    {
        if (source.IsCancellationRequested)
            return;

        var tasks = analysis.CompletedSteps.Select(step => HasGetRootStep(step, source));
        await Task.WhenAll(tasks);
    }

    private async Task HasGetRootStep(PredicateAnalysis analysis, CancellationTokenSource source)
    {
        if (source.IsCancellationRequested)
            return;

        var tasks = analysis.CompletedSteps.Select(step => HasGetRootStep(step, source));
        await Task.WhenAll(tasks);
    }

    private async Task<bool> HasGetRootStep(IPredicateAnalyzerStep step, CancellationTokenSource source)
    {
        if (source.IsCancellationRequested)
            return false;
        if (step is not GetRootIdParameterNameFromPredicates parameterNameStep)
            return false;

        if (!parameterNameStep.IsFound)
            return false;

        await source.CancelAsync();
        RootIdParameterName = parameterNameStep.ParameterName;
        if (Parameters.TryGetValue(RootIdParameterName, out var rootIdStr))
            RootId = Parse(rootIdStr);

        return true;
    }

    private static string SanitizeParameterName(string parameterName)
    {
        return parameterName.Replace("[", string.Empty).Replace("]", string.Empty);
    }
    private int? Parse(string rootIdStr)
        => int.TryParse(rootIdStr.Replace("(", string.Empty).Replace(")", string.Empty), out var rootId) ? rootId : null;
}