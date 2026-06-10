using QueryPlan.Contracts.ShowPlanDotNet.Showplan;

namespace QueryPlan.Analyzer.Contracts.Analyses;

public class PredicateAnalysis
{
    private PredicateAnalysis(ScalarExpression focus, List<IPredicateAnalyzerStep> completedSteps)
    {
        Focus = focus;
        CompletedSteps = completedSteps;
    }

    public static PredicateAnalysis Create(ScalarExpression focus, Queue<IPredicateAnalyzerStep> steps)
    {
        var completedSteps = new List<IPredicateAnalyzerStep>();
        while (steps.TryDequeue(out var step))
        {
            step.Run(focus);
            completedSteps.Add(step);
        }
        return new PredicateAnalysis(focus, completedSteps);
    }

    public ScalarExpression Focus { get; }
    public List<IPredicateAnalyzerStep> CompletedSteps { get; }
}
