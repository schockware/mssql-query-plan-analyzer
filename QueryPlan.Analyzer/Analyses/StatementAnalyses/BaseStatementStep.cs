using QueryPlan.Analyzer.Contracts;
using QueryPlan.Contracts.ShowPlanDotNet.Showplan.RelOps;

namespace QueryPlan.Analyzer.Analyses.StatementAnalyses;

public abstract class BaseStatementStep<TSelf> : IStatementAnalyzerStep<TSelf>
    where TSelf : IStatementAnalyzerStep<TSelf>
{
    private HashSet<Action<TSelf>> Subscribers { get; } = [];

    public abstract Task Run(RelOp relOp);

    protected void Report(TSelf self)
    {
        foreach(var subscriber in Subscribers)
            subscriber(self);
    }

    public void Listen(Action<TSelf> action)
    {
        Subscribers.Add(action);
    }
}