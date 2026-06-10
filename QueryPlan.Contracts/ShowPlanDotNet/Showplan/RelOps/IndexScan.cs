using System.Xml.Serialization;
using QueryPlan.Contracts.ShowPlanDotNet.Showplan;

namespace QueryPlan.Contracts.ShowPlanDotNet.Showplan.RelOps;

public class IndexScan : RelOpBase
{
    [XmlAttribute] public bool Ordered { get; set; }
    [XmlAttribute] public string ScanDirection { get; set; } = string.Empty;

    [XmlElement("SeekPredicates")]
    public SeekPredicates? SeekPredicates { get; set; }

    [XmlElement("Predicate")]
    public ScalarExpression[]? Predicate { get; set; }

    [XmlElement("Object")]
    public ObjectType? Object { get; set; }
}
