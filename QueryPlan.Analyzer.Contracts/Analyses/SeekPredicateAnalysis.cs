using QueryPlan.Contracts.ShowPlanDotNet.Showplan;

namespace QueryPlan.Analyzer.Contracts.Analyses;

public class SeekPredicateAnalysis
{
    private SeekPredicateAnalysis(SeekPredicateBase focus, List<IPredicateAnalyzerStep> completedSteps)
    {
        Focus = focus;
        CompletedSteps = completedSteps;
    }

    public static SeekPredicateAnalysis Create(SeekPredicateBase focus, Queue<IPredicateAnalyzerStep> steps)
    {
        var completedSteps = new List<IPredicateAnalyzerStep>();
        while (steps.TryDequeue(out var step))
        {
            step.Run(focus);
            completedSteps.Add(step);
        }
        return new SeekPredicateAnalysis(focus, completedSteps);
    }

    public SeekPredicateBase Focus { get; }
    public List<IPredicateAnalyzerStep> CompletedSteps { get; }
}
