using System.Xml.Serialization;

namespace QueryPlan.Contracts.ShowPlanDotNet.Showplan.RelOps;

public class ObjectType
{
    [XmlAttribute] public string Database { get; set; } = string.Empty;
    [XmlAttribute] public string Schema { get; set; } = string.Empty;
    [XmlAttribute] public string Table { get; set; } = string.Empty;
    [XmlAttribute] public string Index { get; set; } = string.Empty;
    [XmlAttribute] public string IndexKind { get; set; } = string.Empty;
}
