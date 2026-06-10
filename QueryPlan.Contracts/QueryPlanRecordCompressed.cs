using Encased.Contracts;

namespace QueryPlan.Contracts;

public class QueryPlanRecordCompressed : Uuid7Entity
{
    public QueryPlanRecordCompressed() { }

    public QueryPlanRecordCompressed(Guid sessionId, Server server, PlanSource source, byte[] compressedPlan, Guid textHash)
    {
        SampleSessionId = sessionId;
        Server = server;
        PlanSource = source;
        CompressedPlan = compressedPlan;
        TextHash = textHash;
    }

    public byte[] CompressedPlan { get; set; } = [];
    public Guid SampleSessionId { get; set; }
    public Server Server { get; set; }
    public PlanSource PlanSource { get; set; }
    public Guid TextHash { get; set; }
}
