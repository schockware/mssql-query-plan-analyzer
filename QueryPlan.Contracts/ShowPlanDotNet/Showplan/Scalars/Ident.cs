using System.Xml.Serialization;
using QueryPlan.Contracts.ShowPlanDotNet.Showplan;

namespace QueryPlan.Contracts.ShowPlanDotNet.Showplan.Scalars;

public class Ident
{
    [XmlElement("ColumnReference")]
    public ColumnReference ColumnReference { get; set; } = new();
}
