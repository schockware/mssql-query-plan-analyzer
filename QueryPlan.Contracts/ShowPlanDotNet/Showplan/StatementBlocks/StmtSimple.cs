using System.Xml.Serialization;
using QueryPlan.Contracts.ShowPlanDotNet.Showplan;

namespace QueryPlan.Contracts.ShowPlanDotNet.Showplan.StatementBlocks;

public class StmtSimple : BaseStmtInfoType
{
    [XmlElement("QueryPlan")]
    public QueryPlanNode? QueryPlan { get; set; }
}
