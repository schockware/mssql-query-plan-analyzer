using System.Xml.Serialization;

namespace QueryPlan.Contracts.ShowPlanDotNet.Showplan.RelOps;

public class RunTimeInformationTypeRunTimeCountersPerThread
{
    [XmlAttribute] public int Thread { get; set; }
    [XmlAttribute] public ulong ActualRows { get; set; }
    [XmlAttribute] public ulong ActualRowsRead { get; set; }
    [XmlAttribute] public ulong ActualExecutions { get; set; }
}
