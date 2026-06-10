using System.Xml.Serialization;
using QueryPlan.Contracts.ShowPlanDotNet.Showplan.Scalars;

namespace QueryPlan.Contracts.ShowPlanDotNet.Showplan;

public class ScanRange
{
    [XmlAttribute] public CompareOp? ScanType { get; set; }
    [XmlAttribute] public bool ScanTypeSpecified { get; set; }

    [XmlElement("ColumnReference")]
    public ColumnReference[] RangeColumns { get; set; } = [];

    [XmlElement("ScalarOperator")]
    public Scalar[] RangeExpressions { get; set; } = [];
}
