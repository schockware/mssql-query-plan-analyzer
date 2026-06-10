using Encased.Access;
using Encased.Access.MsSql;
using QueryPlan.Contracts;

namespace QueryPlan.Sampler;

public class CompressedCachePlanSampler(CacheAccessor sourceAccess, IDbAccess analysisAccess) 
    : ICachePlanSampler
{
    public async Task LoadSamples()
    {
        var sessionRepo = analysisAccess.GetRepository<SampleSession>();

        var session = new SampleSession();
        await sessionRepo.SaveAsync(session);

        await LoadSamples(Server.Reporting, session, await sourceAccess.GetReportingReader());
        await LoadSamples(Server.Replica, session, await sourceAccess.GetReplicaReader());
        await LoadSamples(Server.Primary, session, await sourceAccess.GetPrimaryReader());
    }

    private async Task LoadSamples(Server server, SampleSession session, ICacheReader reader)
    {
        var queryPlanRecordRepo = analysisAccess.GetRepository<QueryPlanRecordCompressed>();
        var queryTextRepo = analysisAccess.GetRepository<QueryTextCompressed>();

        while (reader.HasPendingRecords)
        {
            var start = DateTime.Now;
            var batch = await reader.NextAsync();

            var queryTexts = new List<QueryTextCompressed>();
            var records = new List<QueryPlanRecordCompressed>();
            foreach (var item in batch)
            {
                var queryText = new QueryTextCompressed(server, PlanSource.Cache, item.CompressedText);
                var record = new QueryPlanRecordCompressed(session.Id, server, PlanSource.Cache, item.CompressedPlan,
                    queryText.TextHash);
                queryTexts.Add(queryText);
                records.Add(record);
            }

            await queryTextRepo.SaveBulkAsync(queryTexts.DistinctBy(t => t.Id).ToList());
            await queryPlanRecordRepo.SaveBulkAsync(records);

            var elapsed = (DateTime.Now - start).TotalSeconds;
            Console.WriteLine($"{server}: {elapsed} seconds elapsed. {reader.Index} of {reader.TotalRecords}");
            Console.WriteLine(
                $"Estimated time remaining: {(reader.TotalRecords - reader.Index) * elapsed / 60 / CacheReader.RecordsPerBatch} minutes");
        }
    }
}