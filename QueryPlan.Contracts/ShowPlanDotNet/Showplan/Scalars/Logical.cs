using System.Xml.Serialization;

namespace QueryPlan.Contracts.ShowPlanDotNet.Showplan.Scalars;

public class Logical
{
    [XmlAttribute] public string Operation { get; set; } = string.Empty;

    [XmlElement("ScalarOperator")]
    public Scalar[] ScalarOperator { get; set; } = [];
}
