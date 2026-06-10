using QueryPlan.Analyzer.Contracts;
using QueryPlan.Contracts.ShowPlanDotNet.Showplan;
using QueryPlan.Contracts.ShowPlanDotNet.Showplan.Scalars;

namespace QueryPlan.Analyzer.Analyses.StatementAnalyses.PredicateAnalyses;

public abstract class BasePredicateStep : IPredicateAnalyzerStep
{
    protected abstract void Run(Scalar scalar);
    protected abstract void Run(ScanRange range);
    protected abstract void Run(Compare compare);
    
    public abstract void Run(SeekPredicateBase seekPredicate);

    public abstract void Run(ScalarExpression predicate);
}