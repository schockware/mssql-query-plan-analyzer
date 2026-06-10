using QueryPlan.Analyzer.Contracts;
using QueryPlan.Contracts.ShowPlanDotNet.Showplan;
using QueryPlan.Contracts.ShowPlanDotNet.Showplan.RelOps;

namespace QueryPlan.Analyzer.Analyses.StatementAnalyses;

public class AnalyzeRowsRead : BaseStatementStep<AnalyzeRowsRead>
{
    public bool Skip => RuntimeInformation == null || RuntimeInformation.Length == 0;
    public string TableName { get; set; } = string.Empty;
    public RunTimeInformationTypeRunTimeCountersPerThread[] RuntimeInformation { get; set; } = [];
    public ulong TotalActualRowsRead { get; set; }
    public override Task Run(RelOp relOp)
    {
        TableName = relOp.OutputList.FirstOrDefault()?.Table ?? string.Empty;
        RuntimeInformation = relOp.RunTimeInformation ?? [];
        foreach(var thread in RuntimeInformation)
            Tally(thread);
        return Task.CompletedTask;
    }

    private void Tally(RunTimeInformationTypeRunTimeCountersPerThread thread)
    {
        TotalActualRowsRead += thread.ActualRowsRead;
    }
}