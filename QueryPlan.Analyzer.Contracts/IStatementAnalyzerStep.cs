using QueryPlan.Contracts.ShowPlanDotNet.Showplan.RelOps;

namespace QueryPlan.Analyzer.Contracts;

public interface IStatementAnalyzerStep
{
    Task Run(RelOp relOp);
}
public interface IStatementAnalyzerStep<out TSelf> : IStatementAnalyzerStep
    where TSelf : IStatementAnalyzerStep<TSelf>
{
    
    void Listen(Action<TSelf> action);
}