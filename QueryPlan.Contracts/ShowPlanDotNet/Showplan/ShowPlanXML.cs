using System.Xml.Serialization;
using QueryPlan.Contracts.ShowPlanDotNet.Showplan.StatementBlocks;

namespace QueryPlan.Contracts.ShowPlanDotNet.Showplan;

[XmlRoot("ShowPlanXML", Namespace = Ns)]
public class ShowPlanXML
{
    internal const string Ns = "http://schemas.microsoft.com/sqlserver/2004/07/showplan";

    [XmlArray("BatchSequence")]
    [XmlArrayItem("Batch")]
    public BatchType[] Batches { get; set; } = [];

    // Indexed as BatchSequence[batchIndex][stmtBlockIndex]
    // Each batch has exactly one Statements element, so inner array always has one entry.
    [XmlIgnore]
    public StmtBlockType[][] BatchSequence =>
        Batches.Select(b => new[] { b.Statements ?? new StmtBlockType() }).ToArray();
}

public class BatchType
{
    [XmlElement("Statements")]
    public StmtBlockType? Statements { get; set; }
}
