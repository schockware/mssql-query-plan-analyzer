using QueryPlan.Contracts.ShowPlanDotNet.Showplan;
using QueryPlan.Contracts.ShowPlanDotNet.Showplan.Scalars;

namespace QueryPlan.Analyzer.Analyses.StatementAnalyses.PredicateAnalyses;

public class OnlyHasRootIdSeekPredicate: BasePredicateStep
{
    public bool IsFound { get; private set; }
    public List<string> TableNames { get; private set; } = [];

    /// <summary>
    /// Not enough information to determine if it only has an RootId predicate
    /// </summary>
    protected override void Run(Compare compare)
    {
        //no-op
    }

    public override void Run(SeekPredicateBase predicate)
    {
        if (predicate.OnlyHasRootIdPredicate())
        {
            IsFound = true;
            TableNames.AddRange(predicate.GetTableNames());
        }
    }
    /// <summary>
    /// Not enough information to determine if it only has an RootId predicate
    /// </summary>
    public override void Run(ScalarExpression predicate)
    {
        //no-op
    }
    
    /// <summary>
    /// Not enough information to determine if it only has an RootId predicate
    /// </summary>
    protected override void Run(Scalar predicate)
    {
        //no-op
    }
    
    /// <summary>
    /// Not enough information to determine if it only has an RootId predicate
    /// </summary>
    protected override void Run(ScanRange range)
    {
        //no-op
    }
}