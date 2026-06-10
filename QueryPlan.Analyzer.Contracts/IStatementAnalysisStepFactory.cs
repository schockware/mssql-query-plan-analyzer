using QueryPlan.Analyzer.Contracts.Analyses;
using QueryPlan.Contracts.ShowPlanDotNet.Showplan.RelOps;

namespace QueryPlan.Analyzer.Contracts;

public interface IStatementAnalysisStepFactory
{
    StatementAnalysis Build(RelOp focus);
}