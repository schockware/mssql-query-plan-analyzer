using QueryPlan.Contracts.ShowPlanDotNet.Showplan;
using QueryPlan.Contracts.ShowPlanDotNet.Showplan.Scalars;

namespace QueryPlan.Contracts.ShowPlanDotNet.Showplan;

public static class SeekPredicateExtensions
{
    public static bool HasNonParameterizedPredicate(this SeekPredicateBase predicate, out IEnumerable<string> columnNames)
    {
        var found = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (predicate is SeekPredicateNew seekNew)
        {
            foreach (var key in seekNew.SeekKeys)
            {
                CheckRange(key.StartRange, found);
                CheckRange(key.EndRange, found);
                CheckRange(key.Prefix, found);
            }
        }
        columnNames = found;
        return found.Count > 0;
    }

    public static bool OnlyHasRootIdPredicate(this SeekPredicateBase predicate)
    {
        if (predicate is not SeekPredicateNew seekNew) return false;
        foreach (var key in seekNew.SeekKeys)
        {
            var ranges = new[] { key.Prefix, key.StartRange, key.EndRange }
                .Where(r => r != null)
                .Cast<ScanRange>();
            foreach (var range in ranges)
            {
                foreach (var col in range.RangeColumns)
                {
                    if (!IsRootId(col.Column)) return false;
                }
            }
        }
        return seekNew.SeekKeys.Length > 0;
    }

    public static IEnumerable<string> GetTableNames(this SeekPredicateBase predicate)
    {
        var tables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (predicate is not SeekPredicateNew seekNew) return tables;
        foreach (var key in seekNew.SeekKeys)
        {
            var ranges = new[] { key.Prefix, key.StartRange, key.EndRange }
                .Where(r => r != null)
                .Cast<ScanRange>();
            foreach (var range in ranges)
                foreach (var col in range.RangeColumns)
                    if (!string.IsNullOrEmpty(col.Table))
                        tables.Add(col.Table);
        }
        return tables;
    }

    private static void CheckRange(ScanRange? range, HashSet<string> found)
    {
        if (range == null) return;
        foreach (var expr in range.RangeExpressions)
            if (IsLiteral(expr, out var col) && col != null)
                found.Add(col);
    }

    private static bool IsLiteral(Scalar scalar, out string? columnName)
    {
        columnName = null;
        if (scalar.Item is Const) { columnName = "?"; return true; }
        return false;
    }

    private static bool IsRootId(string columnName) =>
        columnName.Contains("RootId", StringComparison.OrdinalIgnoreCase);
}
