using System.Xml.Serialization;

namespace QueryPlan.Contracts.ShowPlanDotNet.Showplan;

public class StatisticsInfo
{
    [XmlAttribute] public string Database { get; set; } = string.Empty;
    [XmlAttribute] public string Schema { get; set; } = string.Empty;
    [XmlAttribute] public string Table { get; set; } = string.Empty;
    [XmlAttribute] public string Statistics { get; set; } = string.Empty;
}
