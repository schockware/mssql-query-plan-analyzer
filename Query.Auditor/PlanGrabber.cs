using Encased.Access.MsSql;
using QueryPlan.Analyzer;
using QueryPlan.Analyzer.Hypotheses;
using QueryPlan.Contracts;
using QueryPlan.Contracts.ShowPlanDotNet.Showplan.StatementBlocks;

namespace Query.Auditor;

/// <summary>
/// We want to hunt for the first plan that only has an RootId seek predicate
/// </summary>
public class PlanGrabber(RawQueryAccessor sourceAccessor)
{
    public async Task GetIt()
    {
        var rootIds = await sourceAccessor.GetRootIds();
        foreach (var rootId in rootIds)
        {
            Console.WriteLine($"Processing: {rootId}");
            var plan = await sourceAccessor.GetActualPlanForQuery(rootId);
            var record = new QueryPlanRecord()
            {
                Plan = plan
            };
            if (!record.TryGetPlanXml(out var showPlan)
                || showPlan.BatchSequence[0][0].Items[0] is not StmtSimple simple)
            {
                Console.WriteLine($"Invalid Plan: {rootId}");
                return;
            }

            var results = await new QueryPlanAnalyzer(record.Id, simple.QueryPlan).AnalyzeAsync();
            if (WillBreakForBiggestClient.IsTrue(results))
            {
                Console.WriteLine($"Will break for largest client. {rootId} generated a plan with a RootId-only seek predicate.");
                var outputFolder = Environment.GetEnvironmentVariable("QPA_UNCOMPRESSED_FOLDER")
                    ?? throw new InvalidOperationException("QPA_UNCOMPRESSED_FOLDER environment variable is not set.");
                File.WriteAllText(Path.Combine(outputFolder, $"confirm-{record.Id}.sqlplan"), plan);
            }
        }
    }
}
