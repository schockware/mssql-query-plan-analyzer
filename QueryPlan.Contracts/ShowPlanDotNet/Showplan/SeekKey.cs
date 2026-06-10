using System.Xml.Serialization;

namespace QueryPlan.Contracts.ShowPlanDotNet.Showplan;

public class SeekKey
{
    [XmlElement("Prefix")]
    public ScanRange? Prefix { get; set; }

    [XmlElement("StartRange")]
    public ScanRange? StartRange { get; set; }

    [XmlElement("EndRange")]
    public ScanRange? EndRange { get; set; }
}
