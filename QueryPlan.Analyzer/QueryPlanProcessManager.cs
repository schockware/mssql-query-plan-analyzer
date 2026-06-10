using QueryPlan.Analyzer.Contracts;
using QueryPlan.Analyzer.Contracts.Analyses;
using QueryPlan.Contracts.ShowPlanDotNet.Showplan;
using QueryPlan.Contracts.ShowPlanDotNet.Showplan.RelOps;

namespace QueryPlan.Analyzer;

public enum ProcessPhase
{
    Pending,
    Initializing,
    AnalyzingPlan,
    AnalyzingStatements,
    ResolvingDependencies,
    Completed,
}

public class QueryPlanProcessManager : IAnalysisResults
{
    private readonly List<IQueryPlanAnalyzerStep> _pendingPlanSteps;
    private readonly Queue<StatementAnalysis> _pendingStatementAnalyses = new();
    private readonly IStatementAnalysisStepFactory _statementStepsFactory;
    private readonly IPredicateAnalysisStepFactory _predicateStepsFactory;

    private readonly List<IQueryPlanAnalyzerStep> _completedPlanSteps = [];
    private readonly List<StatementAnalysis> _completedStatementAnalyses = [];
    private readonly List<SeekPredicateAnalysis> _seekPredicateAnalyses = [];
    private readonly List<PredicateAnalysis> _predicateAnalyses = [];

    public ProcessPhase CurrentPhase { get; private set; } = ProcessPhase.Pending;

    public Guid RecordId { get; }
    public QueryPlanNode Plan { get; }

    public IReadOnlyList<IQueryPlanAnalyzerStep> CompletedPlanSteps => _completedPlanSteps;
    public IReadOnlyList<StatementAnalysis> CompletedStatementAnalyses => _completedStatementAnalyses;
    public IReadOnlyList<SeekPredicateAnalysis> SeekPredicateAnalyses => _seekPredicateAnalyses;
    public IReadOnlyList<PredicateAnalysis> PredicateAnalyses => _predicateAnalyses;

    public QueryPlanProcessManager(
        Guid recordId,
        QueryPlanNode plan,
        IEnumerable<IQueryPlanAnalyzerStep> planSteps,
        IStatementAnalysisStepFactory statementStepsFactory,
        IPredicateAnalysisStepFactory predicateStepsFactory)
    {
        RecordId = recordId;
        Plan = plan;
        _pendingPlanSteps = planSteps.ToList();
        _statementStepsFactory = statementStepsFactory;
        _predicateStepsFactory = predicateStepsFactory;
    }

    public async Task RunAsync()
    {
        if (CurrentPhase != ProcessPhase.Pending)
            throw new InvalidOperationException(
                $"Process manager for record {RecordId} cannot be run from phase {CurrentPhase}.");

        // Phase 1: BFS traverse the RelOp tree, building statement analysis queue
        //          and eagerly running predicate analysis at each leaf node.
        CurrentPhase = ProcessPhase.Initializing;
        if (Plan?.RelOp != null)
            TraverseRelOps(Plan.RelOp);

        // Phase 2: Run plan-level steps against the root plan node.
        CurrentPhase = ProcessPhase.AnalyzingPlan;
        foreach (var step in _pendingPlanSteps)
        {
            step.Run(Plan);
            _completedPlanSteps.Add(step);
        }

        // Phase 3: Run statement-level steps for each relop in traversal order.
        CurrentPhase = ProcessPhase.AnalyzingStatements;
        while (_pendingStatementAnalyses.TryDequeue(out var analysis))
        {
            while (analysis.TryGetNextStatementStep(out var step))
                await step.Run(analysis.Focus);
            _completedStatementAnalyses.Add(analysis);
        }

        // Phase 4: Allow plan steps to resolve cross-phase dependencies
        //          now that predicate and statement results are fully populated.
        CurrentPhase = ProcessPhase.ResolvingDependencies;
        foreach (var step in _completedPlanSteps)
            await step.HandleDependencies(this);

        CurrentPhase = ProcessPhase.Completed;
    }

    private void TraverseRelOps(RelOp root)
    {
        _pendingStatementAnalyses.Enqueue(_statementStepsFactory.Build(root));
        ProcessPredicates(root.Item);

        if (root.Item.RelOp == null)
            return;

        var queue = new Queue<RelOp>(root.Item.RelOp);
        while (queue.TryDequeue(out var relOp))
        {
            _pendingStatementAnalyses.Enqueue(_statementStepsFactory.Build(relOp));
            ProcessPredicates(relOp.Item);

            if (relOp.Item.RelOp != null)
                foreach (var child in relOp.Item.RelOp)
                    queue.Enqueue(child);
        }
    }

    private void ProcessPredicates(RelOpBase relOpBase)
    {
        switch (relOpBase)
        {
            case IndexScan indexScan:
                if (indexScan.SeekPredicates != null)
                    foreach (var seekPredicate in indexScan.SeekPredicates.Items)
                        _seekPredicateAnalyses.Add(_predicateStepsFactory.Build(seekPredicate));
                if (indexScan.Predicate != null)
                    foreach (var scalarExpression in indexScan.Predicate)
                        _predicateAnalyses.Add(_predicateStepsFactory.Build(scalarExpression));
                break;
            case TableScan tableScan:
                if (tableScan.Predicate != null)
                    _predicateAnalyses.Add(_predicateStepsFactory.Build(tableScan.Predicate));
                break;
        }
    }
}
