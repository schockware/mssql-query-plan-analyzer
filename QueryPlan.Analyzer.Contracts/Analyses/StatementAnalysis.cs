using System.Diagnostics.CodeAnalysis;
using QueryPlan.Contracts.ShowPlanDotNet.Showplan.RelOps;

namespace QueryPlan.Analyzer.Contracts.Analyses;

public class StatementAnalysis(RelOp focus, IEnumerable<IStatementAnalyzerStep> steps)
{
    public RelOp Focus { get; } = focus;
    public Queue<IStatementAnalyzerStep> StatementAnalyzerSteps { get; } = new Queue<IStatementAnalyzerStep>(steps);

    public List<IStatementAnalyzerStep> CompletedStatementAnalyzerSteps { get; } = [];
    
    public bool TryGetNextStatementStep([MaybeNullWhen(false)]out IStatementAnalyzerStep step)
    {
        if (!StatementAnalyzerSteps.TryDequeue(out step)) return false;
        
        CompletedStatementAnalyzerSteps.Add(step);
        return true;
    }
}