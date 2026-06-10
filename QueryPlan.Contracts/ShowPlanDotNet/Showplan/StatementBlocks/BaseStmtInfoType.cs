using System.Xml.Serialization;

namespace QueryPlan.Contracts.ShowPlanDotNet.Showplan.StatementBlocks;

public abstract class BaseStmtInfoType
{
    [XmlAttribute] public string StatementText { get; set; } = string.Empty;
    [XmlAttribute] public int StatementId { get; set; }
    [XmlAttribute] public int StatementCompId { get; set; }
    [XmlAttribute] public string StatementType { get; set; } = string.Empty;
}
