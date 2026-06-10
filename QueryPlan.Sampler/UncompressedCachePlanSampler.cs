using Encased.Access;
using Encased.Access.MsSql;
using QueryPlan.Contracts;

namespace QueryPlan.Sampler;

public class UncompressedCachePlanSampler(CacheAccessor sourceAccess, IDbAccess analysisAccess) 
    : ICachePlanSampler
{
    public async Task LoadSamples()
    {
        var sessionRepo = analysisAccess.GetRepository<SampleSession>();

        var session = new SampleSession();
        await sessionRepo.SaveAsync(session);

        await LoadSamples(Server.Primary, session);
        await LoadSamples(Server.Replica, session);
        await LoadSamples(Server.Reporting, session);
    }

    private async Task LoadSamples(Server server, SampleSession session)
    {
        var queryPlanRecordRepo = analysisAccess.GetRepository<QueryPlanRecord>();
        var queryTextRepo = analysisAccess.GetRepository<QueryText>();

        var reader = server switch
        {
            Server.Replica => await sourceAccess.GetReplicaReader(),
            Server.Reporting => await sourceAccess.GetReportingReader(),
            _ => await sourceAccess.GetPrimaryReader(),
        };
        while (reader.HasPendingRecords)
        {
            var start = DateTime.Now;
            var batch = await reader.NextAsync();

            var queryTexts = new List<QueryText>();
            var records = new List<QueryPlanRecord>();
            foreach (var item in batch)
            {
                var queryText = new QueryText(server, PlanSource.Cache, item.CompressedText);
                var record = new QueryPlanRecord(session.Id, server, PlanSource.Cache, item.CompressedPlan,
                    queryText.TextHash);
                queryTexts.Add(queryText);
                records.Add(record);
            }

            await queryTextRepo.SaveBulkAsync(queryTexts);
            await queryPlanRecordRepo.SaveBulkAsync(records);

            var elapsed = (DateTime.Now - start).TotalSeconds;
            Console.WriteLine($"{elapsed} seconds elapsed. {reader.Index} of {reader.TotalRecords}");
            Console.WriteLine(
                $"Estimated time remaining: {(reader.TotalRecords - reader.Index) * elapsed / 60 / CacheReader.RecordsPerBatch} minutes");
        }
    }
}