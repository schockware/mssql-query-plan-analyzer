using QueryPlan.Contracts.ShowPlanDotNet.Showplan.Scalars;

namespace QueryPlan.Contracts.ShowPlanDotNet.Showplan.Scalars;

public static class ScalarExtensions
{
    public static bool HasNonParameterizedPredicate(this Scalar scalar, out string columnName)
    {
        columnName = string.Empty;
        return scalar.Item switch
        {
            Compare compare => compare.HasNonParameterizedPredicate(out columnName),
            Const => true,
            _ => false,
        };
    }

    public static bool HasNonParameterizedPredicate(this Compare compare, out string columnName)
    {
        columnName = string.Empty;
        // A non-parameterized predicate has a Const on one side and an Ident (column ref) on the other
        var operands = compare.ScalarOperator;
        if (operands.Length != 2) return false;

        var hasConst = operands.Any(s => s.Item is Const);
        if (!hasConst) return false;

        var identOp = operands.FirstOrDefault(s => s.Item is Ident);
        if (identOp?.Item is Ident ident)
            columnName = ident.ColumnReference.Column;
        else
            columnName = "?";

        return true;
    }
}
