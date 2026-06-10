using System.Xml.Serialization;

namespace QueryPlan.Contracts.ShowPlanDotNet.Showplan.RelOps;

public abstract class RelOpBase
{
    [XmlElement("RelOp")]
    public RelOp[]? RelOp { get; set; }
}
