using AppStatistics.Contracts.TableSizes;
using QueryPlan.Analyzer.Contracts;
using QueryPlan.Analyzer.Hypotheses;

namespace QueryPlan.Analyzer.MCP;

public static class HypothesisRegistry
{
    /// <summary>
    /// Hypotheses that require no external dependencies. Instantiated directly.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, Func<IHypothesis>> Available =
        new Dictionary<string, Func<IHypothesis>>(StringComparer.OrdinalIgnoreCase)
        {
            ["BasicStatistics"] = () => new BasicStatistics(),
            ["SmallClientParameterSniffingIsRising"] = () => new SmallClientParameterSniffingIsRising(),
            ["WillBreakForBiggestClient"] = () => new WillBreakForBiggestClient(),
            ["RisingParameterSniffingIncreasesRatesForBadQueryPlans"] = () => new RisingParameterSniffingIncreasesRatesForBadQueryPlans(),
            ["NonParameterizedPredicatesCausingPlanCacheBloom"] = () => new NonParameterizedPredicatesCausingPlanCacheBloom(),
            ["TopQueriesResponsibleForBiggestClientRisk"] = () => new TopQueriesResponsibleForBiggestClientRisk(),
        };

    /// <summary>
    /// Hypotheses that require <see cref="ITableSizeCatalog"/> to be resolved from DI before use.
    /// </summary>
    public static IReadOnlyDictionary<string, Func<ITableSizeCatalog, IHypothesis>> AvailableWithCatalog =>
        new Dictionary<string, Func<ITableSizeCatalog, IHypothesis>>(StringComparer.OrdinalIgnoreCase)
        {
            ["IndexSeekIsEffectivelyAFullScan"] = catalog => new IndexSeekIsEffectivelyAFullScan(new HighPercentageOfRowsReadFromIndex(catalog)),
            ["LargeClientPlanAdoptedBySmallClient"] = catalog => new LargeClientPlanAdoptedBySmallClient(catalog),
        };
}
