using System.Xml.Serialization;
using QueryPlan.Contracts.ShowPlanDotNet.Showplan;

namespace QueryPlan.Contracts.ShowPlanDotNet.Showplan.RelOps;

public class TableScan : RelOpBase
{
    [XmlElement("Predicate")]
    public ScalarExpression? Predicate { get; set; }

    [XmlElement("Object")]
    public ObjectType? Object { get; set; }
}
