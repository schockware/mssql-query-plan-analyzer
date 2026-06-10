using System.Xml.Serialization;

namespace QueryPlan.Contracts.ShowPlanDotNet.Showplan.Scalars;

public class Compare
{
    [XmlAttribute] public CompareOp CompareOp { get; set; }

    [XmlElement("ScalarOperator")]
    public Scalar[] ScalarOperator { get; set; } = [];
}
