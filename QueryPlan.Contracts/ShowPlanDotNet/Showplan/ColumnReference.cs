using System.Xml.Serialization;

namespace QueryPlan.Contracts.ShowPlanDotNet.Showplan;

public class ColumnReference
{
    [XmlAttribute] public string Database { get; set; } = string.Empty;
    [XmlAttribute] public string Schema { get; set; } = string.Empty;
    [XmlAttribute] public string Table { get; set; } = string.Empty;
    [XmlAttribute] public string Column { get; set; } = string.Empty;
    [XmlAttribute] public string ParameterCompiledValue { get; set; } = string.Empty;
    [XmlAttribute] public string ParameterRuntimeValue { get; set; } = string.Empty;
}
