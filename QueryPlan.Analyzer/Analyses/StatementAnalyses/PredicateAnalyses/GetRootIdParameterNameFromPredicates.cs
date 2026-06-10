using QueryPlan.Contracts.ShowPlanDotNet.Showplan;
using QueryPlan.Contracts.ShowPlanDotNet.Showplan.Scalars;

namespace QueryPlan.Analyzer.Analyses.StatementAnalyses.PredicateAnalyses;

public class GetRootIdParameterNameFromPredicates : BasePredicateStep
{
    private const string RootIdColumnName = "RootId";
    public string ParameterName { get; private set; } = string.Empty;
    public bool IsFound => ParameterName != string.Empty;

    public override void Run(SeekPredicateBase predicate)
    {
        if (predicate is not SeekPredicateNew seekNew)
            return;

        foreach (var key in seekNew.SeekKeys)
        {
            if (key?.Prefix?.ScanType == null)
                continue;
            if (key.Prefix.ScanType != CompareOp.Eq)
                continue;

            if(key?.StartRange == null || key?.EndRange == null)
                continue;
            
            Run(key.StartRange);
            Run(key.EndRange);
        }
    }

    public override void Run(ScalarExpression predicate)
    {
        Run(predicate.ScalarOperator);
    }
    
    protected override void Run(Scalar predicate)
    {
        switch (predicate.Item)
        {
            case Compare { CompareOp: CompareOp.Eq } compare:
                Run(compare);
                break;
            case Logical logical:
            {
                foreach (var op in logical.ScalarOperator)
                    Run(op);
                break;
            }
        }
    }

    protected override void Run(ScanRange range)
    {
        if (range?.RangeColumns == null || range?.RangeExpressions == null)
            return;
        
        for (var i = 0; i < range.RangeColumns.Length; i++)
        {
            var column = range.RangeColumns[i];
            if (column.Column != RootIdColumnName)
                continue;

            var expression = range.RangeExpressions[i];
            if (expression.Item is Ident ident)
                Found(ident.ColumnReference.Column);
        }
    }

    protected override void Run(Compare compare)
    {
        var scalars = compare.ScalarOperator;
        if (scalars.Length != 2)
            return;
        if (scalars[0].Item is Ident { ColumnReference.Column: RootIdColumnName }
            && scalars[1].Item is Ident rightIdent)
            Found(rightIdent.ColumnReference.Column);
    }

    private void Found(string parameterName)
    {
        ParameterName = parameterName;
    }
}