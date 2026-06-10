using QueryPlan.Analyzer.Contracts;

namespace QueryPlan.Analyzer.Hypotheses;

public class RisingParameterSniffingIncreasesRatesForBadQueryPlans : IHypothesis
{
    public string Name => "RisingParameterSniffingIncreasesRatesForBadQueryPlans";
    public string Description => "Hypothesis under development: correlates rising parameter sniffing rates with bad plan adoption.";

    public Task Hypothesize(IAnalysisResults manifest) => Task.CompletedTask;

    public string Summarize() => "This hypothesis is not yet implemented.";
}
