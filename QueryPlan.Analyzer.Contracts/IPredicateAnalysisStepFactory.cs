using QueryPlan.Analyzer.Contracts.Analyses;
using QueryPlan.Contracts.ShowPlanDotNet.Showplan;

namespace QueryPlan.Analyzer.Contracts;

public interface IPredicateAnalysisStepFactory
{
    PredicateAnalysis Build(ScalarExpression focus);

    SeekPredicateAnalysis Build(SeekPredicateBase focus);
}