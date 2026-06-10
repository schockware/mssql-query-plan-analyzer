using AppStatistics.Contracts.TableSizes;
using QueryPlan.Analyzer.Analyses;
using QueryPlan.Analyzer.Analyses.StatementAnalyses;
using QueryPlan.Analyzer.Contracts;

namespace QueryPlan.Analyzer.Hypotheses;

public class HighPercentageOfRowsReadFromIndex(ITableSizeCatalog catalog)
{
    public async Task<IEnumerable<Result>> Check(IAnalysisResults results)
    {
        var paramSniffing =
            results.CompletedPlanSteps.FirstOrDefault(step => step is AnalyzeParameterSniffing) as
                AnalyzeParameterSniffing;
        var rootId = paramSniffing?.RootId;
        if (rootId == null)
            return [];

        var tasks = results.CompletedStatementAnalyses.SelectMany(op =>
            op.CompletedStatementAnalyzerSteps.Where(step => step is AnalyzeRowsRead).Select(step =>
                step is AnalyzeRowsRead rowsRead
                    ? Check(rootId.Value, results.RecordId, rowsRead)
                    : Task.FromResult(new Result(rootId.Value, string.Empty, results.RecordId) { Skip = true })));

        return await Task.WhenAll(tasks);
    }

    private Task<Result> Check(int rootId, Guid recordId, AnalyzeRowsRead rowsRead)
    {
        if (rowsRead.Skip)
            return Task.FromResult(new Result(rootId, string.Empty, recordId) { Skip = true });

        var result = new Result(rootId, rowsRead.TableName, recordId);

        var tableSize = catalog.GetTableSize(rowsRead.TableName);
        result.WholeTablePercentage = (float)tableSize.Records / rowsRead.TotalActualRowsRead;

        var rootTableSize = catalog.GetRootTableSize(rootId, rowsRead.TableName);
        result.RootPercentage = (float)rootTableSize.Records / rowsRead.TotalActualRowsRead;
        return Task.FromResult(result);
    }

    public class Result(int rootId, string tableName, Guid recordId)
    {
        public Guid RecordId { get; } = recordId;
        public int RootId { get; } = rootId;
        public string TableName { get; } = tableName;
        public float WholeTablePercentage { get; set; }
        public float RootPercentage { get; set; }
        public bool Skip { get; set; }

        public Threshold WholeTableThreshold
        {
            get
            {
                if (Skip) return Threshold.None;
                return WholeTablePercentage switch
                {
                    > 1f => Threshold.Over,
                    > 0.97f => Threshold.All,
                    > 0.90f => Threshold.High,
                    > 0.75f => Threshold.Medium,
                    > 0.25f => Threshold.Low,
                    > 0.10f => Threshold.Minimal,
                    _ => Threshold.None
                };
            }
        }

        public Threshold RootThreshold
        {
            get
            {
                if (Skip) return Threshold.None;
                return RootPercentage switch
                {
                    > 1f => Threshold.Over,
                    > 0.97f => Threshold.All,
                    > 0.90f => Threshold.High,
                    > 0.75f => Threshold.Medium,
                    > 0.25f => Threshold.Low,
                    > 0.10f => Threshold.Minimal,
                    _ => Threshold.None
                };
            }
        }
    }

    public enum Threshold
    {
        None,
        Minimal,
        Low,
        Medium,
        High,
        All,
        Over,
    }
}
