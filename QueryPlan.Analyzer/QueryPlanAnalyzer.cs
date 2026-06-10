using QueryPlan.Analyzer.Analyses;
using QueryPlan.Analyzer.Contracts;
using QueryPlan.Contracts.ShowPlanDotNet.Showplan;

namespace QueryPlan.Analyzer;

public class QueryPlanAnalyzer(Guid recordId, QueryPlanNode plan)
{
    public async Task<IAnalysisResults> AnalyzeAsync()
    {
        var manager = new QueryPlanProcessManager(
            recordId,
            plan,
            [new AnalyzeParameterSniffing(), new IsOurQuery()],
            new StatementAnalysisStepFactory(),
            new PredicateAnalysisStepFactory());

        await manager.RunAsync();
        return manager;
    }
}
