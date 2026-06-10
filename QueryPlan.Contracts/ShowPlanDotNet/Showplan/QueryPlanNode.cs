using System.Xml.Serialization;
using QueryPlan.Contracts.ShowPlanDotNet.Showplan.RelOps;

namespace QueryPlan.Contracts.ShowPlanDotNet.Showplan;

public class QueryPlanNode
{
    [XmlElement("RelOp")]
    public RelOp? RelOp { get; set; }

    [XmlArray("ParameterList")]
    [XmlArrayItem("ColumnReference")]
    public ColumnReference[] ParameterList { get; set; } = [];

    [XmlArray("OptimizerStatsUsage")]
    [XmlArrayItem("StatisticsInfo")]
    public StatisticsInfo[] OptimizerStatsUsage { get; set; } = [];

    [XmlAttribute] public string NonParallelPlanReason { get; set; } = string.Empty;
    [XmlAttribute] public double CachedPlanSize { get; set; }
    [XmlAttribute] public string CompileTime { get; set; } = string.Empty;
}
