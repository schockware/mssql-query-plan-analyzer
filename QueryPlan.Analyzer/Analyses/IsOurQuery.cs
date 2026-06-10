using QueryPlan.Analyzer.Analyses.StatementAnalyses.PredicateAnalyses;
using QueryPlan.Analyzer.Contracts;
using QueryPlan.Analyzer.Contracts.Analyses;
using QueryPlan.Contracts.ShowPlanDotNet.Showplan;

namespace QueryPlan.Analyzer.Analyses;

public class IsOurQuery : IQueryPlanAnalyzerStep
{
    private readonly string[] _systemDbNames = ["[master]", "[msdb]", "[model]", "[tempdb]"];
    private readonly string[] _systemSchemas = ["[sys]"];
    public IQueryPlanAnalyzerStep.AnalysisStatus Status { get; private set; }
    public bool IsSystemDatabase { get; set; }
    public bool IsSystemSchema { get; set; }
    public bool IsFound => !IsSystemDatabase && !IsSystemSchema;

    public void Run(QueryPlanNode plan)
    {
        if (plan?.OptimizerStatsUsage == null)
            return;

        IsSystemDatabase = plan.OptimizerStatsUsage.Any(stat => _systemDbNames.Contains(stat.Database));
        IsSystemSchema =  plan.OptimizerStatsUsage.Any(stat => _systemSchemas.Contains(stat.Schema));
    }

    public Task HandleDependencies(IAnalysisResults results) => Task.CompletedTask;
}