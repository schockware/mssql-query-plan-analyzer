using QueryPlan.Analyzer.Analyses.StatementAnalyses.PredicateAnalyses;
using QueryPlan.Analyzer.Contracts;
using QueryPlan.Analyzer.Contracts.Analyses;
using QueryPlan.Contracts.ShowPlanDotNet.Showplan;

namespace QueryPlan.Analyzer;

public class PredicateAnalysisStepFactory : IPredicateAnalysisStepFactory
{
    public PredicateAnalysis Build(ScalarExpression focus)
        => PredicateAnalysis.Create(focus, GetSteps());

    public SeekPredicateAnalysis Build(SeekPredicateBase focus)
        => SeekPredicateAnalysis.Create(focus, GetSteps());

    private Queue<IPredicateAnalyzerStep> GetSteps()
        => new([
            new GetRootIdParameterNameFromPredicates(), 
            new OnlyHasRootIdSeekPredicate(),
            new HasNonParameterizedPredicate()
        ]);
}