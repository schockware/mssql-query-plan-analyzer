using QueryPlan.Contracts.ShowPlanDotNet.Showplan;

namespace QueryPlan.Analyzer.Contracts;

public interface IPredicateAnalyzerStep
{
    void Run(SeekPredicateBase seekPredicate);
    void Run(ScalarExpression predicate);
}
