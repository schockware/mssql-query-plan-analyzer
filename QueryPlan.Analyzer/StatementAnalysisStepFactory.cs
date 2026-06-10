using QueryPlan.Analyzer.Analyses.StatementAnalyses;
using QueryPlan.Analyzer.Contracts;
using QueryPlan.Analyzer.Contracts.Analyses;
using QueryPlan.Contracts.ShowPlanDotNet.Showplan.RelOps;

namespace QueryPlan.Analyzer;

public class StatementAnalysisStepFactory : IStatementAnalysisStepFactory
{
    public StatementAnalysis Build(RelOp focus)
    {
        return new StatementAnalysis(focus,[new AnalyzeRowsRead()]);
    }
}