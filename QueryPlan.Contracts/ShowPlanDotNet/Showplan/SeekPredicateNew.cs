using System.Xml.Serialization;

namespace QueryPlan.Contracts.ShowPlanDotNet.Showplan;

public class SeekPredicateNew : SeekPredicateBase
{
    [XmlElement("SeekKeys")]
    public SeekKey[] SeekKeys { get; set; } = [];
}
