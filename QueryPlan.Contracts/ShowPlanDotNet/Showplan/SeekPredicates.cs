using System.Xml.Serialization;

namespace QueryPlan.Contracts.ShowPlanDotNet.Showplan;

public class SeekPredicates
{
    [XmlElement("SeekPredicateNew", typeof(SeekPredicateNew))]
    [XmlElement("SeekPredicate", typeof(SeekPredicateBase))]
    public SeekPredicateBase[] Items { get; set; } = [];
}
