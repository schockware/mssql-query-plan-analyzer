using QueryPlan.Analyzer.Contracts.Analyses;

namespace QueryPlan.Analyzer.Contracts;

/// <summary>
/// Read-only view of a completed query plan analysis.
/// Consumed by hypotheses and dependency-resolution steps.
/// </summary>
public interface IAnalysisResults
{
    Guid RecordId { get; }
    IReadOnlyList<IQueryPlanAnalyzerStep> CompletedPlanSteps { get; }
    IReadOnlyList<StatementAnalysis> CompletedStatementAnalyses { get; }
    IReadOnlyList<SeekPredicateAnalysis> SeekPredicateAnalyses { get; }
    IReadOnlyList<PredicateAnalysis> PredicateAnalyses { get; }
}
