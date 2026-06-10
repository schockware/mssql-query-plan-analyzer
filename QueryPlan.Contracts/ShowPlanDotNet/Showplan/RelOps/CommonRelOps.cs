namespace QueryPlan.Contracts.ShowPlanDotNet.Showplan.RelOps;

public class NestedLoops : RelOpBase { }
public class Hash : RelOpBase { }
public class Sort : RelOpBase { }
public class Merge : RelOpBase { }
public class Filter : RelOpBase { }
public class ComputeScalar : RelOpBase { }
public class Top : RelOpBase { }
public class StreamAggregate : RelOpBase { }
public class Parallelism : RelOpBase { }
public class Spool : RelOpBase { }
public class Assert : RelOpBase { }
public class Segment : RelOpBase { }
public class Sequence : RelOpBase { }
public class Update : RelOpBase { }
public class Insert : RelOpBase { }
public class Delete : RelOpBase { }
public class RowCountSpool : RelOpBase { }
public class BatchHashTableBuild : RelOpBase { }
public class WindowAggregate : RelOpBase { }
public class ForeignKeyReferencesCheck : RelOpBase { }
public class Remote : RelOpBase { }
public class RemoteQuery : RelOpBase { }
public class RemoteModify : RelOpBase { }
public class Adaptive : RelOpBase { }
public class UnknownRelOp : RelOpBase { }
