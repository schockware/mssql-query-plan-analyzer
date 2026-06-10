namespace QueryPlan.Analyzer.Contracts;

public interface IHypothesis
{
    string Name { get; }
    string Description { get; }

    Task Hypothesize(IAnalysisResults results);

    Task Hypothesize(IEnumerable<IAnalysisResults> results)
        => Task.WhenAll(results.Select(Hypothesize));

    string Summarize();
}
