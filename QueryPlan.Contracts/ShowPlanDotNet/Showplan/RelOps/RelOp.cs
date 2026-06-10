using System.Xml.Serialization;
using QueryPlan.Contracts.ShowPlanDotNet.Showplan;

namespace QueryPlan.Contracts.ShowPlanDotNet.Showplan.RelOps;

public class RelOp
{
    [XmlAttribute] public string PhysicalOp { get; set; } = string.Empty;
    [XmlAttribute] public string LogicalOp { get; set; } = string.Empty;
    [XmlAttribute] public int NodeId { get; set; }
    [XmlAttribute] public double EstimateRows { get; set; }
    [XmlAttribute] public double EstimatedTotalSubtreeCost { get; set; }

    [XmlArray("OutputList")]
    [XmlArrayItem("ColumnReference")]
    public ColumnReference[] OutputList { get; set; } = [];

    [XmlArray("RunTimeInformation")]
    [XmlArrayItem("RunTimeCountersPerThread")]
    public RunTimeInformationTypeRunTimeCountersPerThread[]? RunTimeInformation { get; set; }

    [XmlElement("IndexScan", typeof(IndexScan))]
    [XmlElement("TableScan", typeof(TableScan))]
    [XmlElement("NestedLoops", typeof(NestedLoops))]
    [XmlElement("Hash", typeof(Hash))]
    [XmlElement("Sort", typeof(Sort))]
    [XmlElement("Merge", typeof(Merge))]
    [XmlElement("Filter", typeof(Filter))]
    [XmlElement("ComputeScalar", typeof(ComputeScalar))]
    [XmlElement("Top", typeof(Top))]
    [XmlElement("StreamAggregate", typeof(StreamAggregate))]
    [XmlElement("Parallelism", typeof(Parallelism))]
    [XmlElement("Spool", typeof(Spool))]
    [XmlElement("Assert", typeof(Assert))]
    [XmlElement("Segment", typeof(Segment))]
    [XmlElement("Sequence", typeof(Sequence))]
    [XmlElement("Update", typeof(Update))]
    [XmlElement("Insert", typeof(Insert))]
    [XmlElement("Delete", typeof(Delete))]
    [XmlElement("RowCountSpool", typeof(RowCountSpool))]
    [XmlElement("BatchHashTableBuild", typeof(BatchHashTableBuild))]
    [XmlElement("WindowAggregate", typeof(WindowAggregate))]
    [XmlElement("ForeignKeyReferencesCheck", typeof(ForeignKeyReferencesCheck))]
    [XmlElement("Remote", typeof(Remote))]
    [XmlElement("RemoteQuery", typeof(RemoteQuery))]
    [XmlElement("RemoteModify", typeof(RemoteModify))]
    [XmlElement("Adaptive", typeof(Adaptive))]
    public RelOpBase Item { get; set; } = new UnknownRelOp();
}
