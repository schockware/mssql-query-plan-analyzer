using QueryPlan.Contracts.ShowPlanDotNet.Showplan;

namespace QueryPlan.Analyzer.Contracts;

public interface IQueryPlanAnalyzerStep
{
    AnalysisStatus Status { get; }

    void Run(QueryPlanNode plan);

    Task HandleDependencies(IAnalysisResults results);

    public enum AnalysisStatus
    {
        Pending,
        HasDependencies,
        Completed,
        Skipped,
        Error,
    }
}
