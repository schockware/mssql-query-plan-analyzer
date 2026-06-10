using System.Xml.Serialization;

namespace QueryPlan.Contracts.ShowPlanDotNet.Showplan.StatementBlocks;

public class StmtBlockType
{
    [XmlElement("StmtSimple", typeof(StmtSimple))]
    [XmlElement("StmtCond", typeof(StmtCond))]
    [XmlElement("StmtCursor", typeof(StmtCursor))]
    [XmlElement("StmtReceive", typeof(StmtReceive))]
    [XmlElement("StmtUseDb", typeof(StmtUseDb))]
    public BaseStmtInfoType[] Items { get; set; } = [];
}

public class StmtCond : BaseStmtInfoType { }
public class StmtCursor : BaseStmtInfoType { }
public class StmtReceive : BaseStmtInfoType { }
public class StmtUseDb : BaseStmtInfoType { }
