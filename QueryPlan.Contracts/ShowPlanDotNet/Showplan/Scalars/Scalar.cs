using System.Xml.Serialization;
using QueryPlan.Contracts.ShowPlanDotNet.Showplan;

namespace QueryPlan.Contracts.ShowPlanDotNet.Showplan.Scalars;

public class Scalar
{
    [XmlElement("Compare", typeof(Compare))]
    [XmlElement("Logical", typeof(Logical))]
    [XmlElement("Identifier", typeof(Ident))]
    [XmlElement("Const", typeof(Const))]
    [XmlElement("Arithmetic", typeof(Arithmetic))]
    [XmlElement("Convert", typeof(Convert))]
    [XmlElement("Intrinsic", typeof(Intrinsic))]
    [XmlElement("IF", typeof(If))]
    [XmlElement("UserDefinedFunction", typeof(UserDefinedFunction))]
    [XmlElement("Aggregate", typeof(Aggregate))]
    [XmlElement("Subquery", typeof(Subquery))]
    [XmlElement("ScalarExpressionList", typeof(ScalarExpressionList))]
    public object? Item { get; set; }
}
