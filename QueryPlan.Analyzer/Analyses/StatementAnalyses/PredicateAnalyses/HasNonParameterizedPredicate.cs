using QueryPlan.Contracts.ShowPlanDotNet.Showplan;
using QueryPlan.Contracts.ShowPlanDotNet.Showplan.Scalars;

namespace QueryPlan.Analyzer.Analyses.StatementAnalyses.PredicateAnalyses;

public class HasNonParameterizedPredicate : BasePredicateStep
{
    public bool IsFound { get; private set; }
    public HashSet<string> ColumnNames { get; } = [];

    protected override void Run(Compare compare)
    {
        var isFound = compare.HasNonParameterizedPredicate(out var columnName);
        IsFound = IsFound || isFound;
        
        if (isFound)
            ColumnNames.Add(columnName);
    }

    public override void Run(SeekPredicateBase predicate)
    {
        IsFound = predicate.HasNonParameterizedPredicate(out var columnNames) || IsFound;

        foreach(var columnName in columnNames)
            ColumnNames.Add(columnName);
    }

    public override void Run(ScalarExpression predicate)
    {
        Run(predicate.ScalarOperator);
    }

    protected override void Run(Scalar predicate)
    {
        var isFound = predicate.HasNonParameterizedPredicate(out var columnName);
        IsFound = IsFound || isFound;
        
        if (isFound)
            ColumnNames.Add(columnName);
    }

    protected override void Run(ScanRange range)
    {
        IsFound = IsFound || range.RangeExpressions.Any(Check);
    }

    private bool Check(Scalar scalar)
    {
        if (!scalar.HasNonParameterizedPredicate(out var columnName)) 
            return false;
        
        ColumnNames.Add(columnName);
        return true;

    }
}