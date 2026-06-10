using System.Xml.Serialization;
using QueryPlan.Contracts.ShowPlanDotNet.Showplan.Scalars;

namespace QueryPlan.Contracts.ShowPlanDotNet.Showplan;

public class ScalarExpression
{
    [XmlElement("ScalarOperator")]
    public Scalar ScalarOperator { get; set; } = new();
}
