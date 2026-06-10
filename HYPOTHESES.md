# Hypotheses — What We Were Chasing

## Context

Multi-tenant SaaS platform. All clients in a single SQL Server database, separated by
a `RootId` / `ClientId` column on every significant table. Entity Framework generated
the majority of queries. Client sizes varied by orders of magnitude — the smallest
tenants had a few hundred rows on core tables; the largest had tens of millions.

When performance degraded, the standard response from the organization was to chase EF
query optimizations and add more indexes. This was making things worse. More indexes
meant more plan cache entries, more compilations competing for the same resources, and
more surfaces for the specific failure modes described below.

The tool existed to generate numbers. Verbal descriptions of parameter sniffing did not
move the needle. Evidence at scale did.

---

## The Scale Problem

Before any specific hypothesis, we needed to establish what was actually happening in
the plan cache. The primary database was creating and destroying **50,000 execution
plans per hour**. The replicated read server was running at double that rate. The
reporting server was all ad-hoc queries — but its load was low enough that it wasn't a
problem.

At 50,000 plans per hour on the primary, the system was operating near the
`RESOURCE_SEMAPHORE_QUERY_COMPILE` ceiling. SQL Server limits simultaneous plan
compilations to roughly `2 × logical CPU count`. Under normal load this is invisible.
Under a spike — a Monday morning cache reset, a large tenant hitting a freshly-evicted
plan — threads queue for the compile semaphore. Throughput doesn't degrade gracefully.
It collapses. And the organization was responding to each collapse by adding more
indexes, which increased plan cache churn and moved the ceiling closer.

Establishing this baseline — the volume, the rate, the proximity to the ceiling — was
the first thing `BasicStatistics` was built to support.

---

## Hypothesis Progression

### BasicStatistics

The opening triage hypothesis. Before you know which problem you're looking at, you
need to know what you're dealing with at the session level.

`BasicStatistics` counts three things across every plan in a session:

- **Parameter-sniffed plans** — plans compiled against one tenant's data and cached for
  reuse by others
- **Root-only seek predicate plans** — index seeks that only filter by `RootId`, missing
  available index columns
- **Non-parameterized predicate plans** — literal values embedded directly in WHERE
  clauses, forcing a unique compilation per unique value

These three numbers were the first thing we looked at when a new session was loaded.
They answer the question: *which of the three problems is dominant right now?*

---

### WillBreakForBiggestClient

The first specific failure mode we needed to prove.

When an EF query generates an index seek that only uses `RootId` as its seek predicate
— ignoring additional indexed columns — SQL Server scans every row belonging to that
tenant within the index. For a small tenant with 500 rows, this is fast. The plan looks
efficient. SQL Server caches it.

When the largest tenant hits that same cached plan, SQL Server scans every row
belonging to *them* within the index. That tenant had tens of millions of rows. The
plan that looked fine for the small tenant is catastrophic at scale — and because it's
cached, every subsequent execution by the largest tenant reuses it.

`WillBreakForBiggestClient` flags every plan in a session where the only seek predicate
is the root/client identifier. These plans are correct in structure but wrong in
consequence for large tenants.

This was the hypothesis that started the conversation. The output wasn't "we think this
might be happening" — it was a count of flagged plan IDs that could be pulled up and
examined.

---

### TopQueriesResponsibleForBiggestClientRisk

Once `WillBreakForBiggestClient` established that root-only seek plans existed at
scale, the next question was: *which tables are causing this repeatedly?*

The organization initially assumed the problem was isolated to a few queries that could
be patched individually. `TopQueriesResponsibleForBiggestClientRisk` ranked tables by
how frequently they appeared in risky plans across a session, alongside the root IDs
compiling those plans. The same three or four tables appeared in hundreds of plans per
session. It was structural, not incidental.

---

### SmallClientParameterSniffingIsRising

The next piece of evidence needed was *who* was compiling the bad plans.

SQL Server embeds the parameter values used at compile time into the cached plan. By
extracting the `RootId` from the compiled plan's seek predicates, we could identify
which tenant triggered the compilation. `SmallClientParameterSniffingIsRising` tracks
every compilation by `RootId` across a session and counts how many plans each tenant
compiled.

The output consistently showed the same pattern: a handful of small tenants were
responsible for the majority of plan compilations. These were high-frequency,
low-volume tenants — scheduled jobs, integrations, automated workflows running against
small datasets. Their plans were fast, looked healthy, and were systematically wrong
for the largest tenant who inherited them.

---

### LargeClientPlanAdoptedBySmallClient

This hypothesis made the mechanism quantitative — and led to the finding that changed
the conversation entirely.

Knowing a plan was compiled by a small tenant is not sufficient. You need to show *how
small* relative to the tenant that will actually use the plan. `LargeClientPlanAdoptedBySmallClient`
joins the plan analysis against live table size data (populated by `AppStatistics.CLI`)
and computes the compiling tenant's row count on each affected table.

The finding: **clients with fewer than 100,000 records had a 30% probability of
compiling a bad plan that would cause a large client to blow out SQL Server.** That
number came from correlating plan compilation data against table sizes across captured
sessions. It was not an assumption — it was measured.

#### The concurrency trajectory

The more significant output from this analysis was a predictive model — a concurrency
trajectory — that calculated exactly when the Monday morning performance pattern would
become a daily one.

Client organisations moved through three phases as they accrued records:

| Phase | Record count | Role |
|---|---|---|
| Perpetrator | < 100,000 | 30% chance of compiling a plan that victimises large clients |
| Neutral | 100,000 – ~1,000,000 | Neither compiling bad plans nor being harmed by them |
| Victim | > ~1,000,000 | Inherits the bad plans compiled by perpetrators |

Every new client acquired started as a perpetrator. As existing clients grew, they
moved from perpetrator to neutral — then at roughly one million records, from neutral
to victim. The largest clients were deep in victim territory and getting worse as their
record counts grew.

The trajectory model projected forward using the historical client acquisition rate and
per-client record growth rate. As more clients crossed into the victim range
simultaneously, the plan cache stopped recovering between incidents. The Monday problem
was on a path to becoming Monday through Friday. The model gave a specific timeframe.

This research was shared with the organisation. The threshold numbers and the
trajectory formula went to the group.

#### Table-level prediction

The same growth rate analysis applied to individual tables. By projecting record growth
per table against the parameter sniffing thresholds, we identified **21 tables that
would cross the victim threshold within 6 months** — tables that were neutral at the
time of analysis but whose growth trajectory would make them the next parameter
sniffing hot spots.

The projection ran quarterly out to 3 years. Six months was the organisational
definition of imminent — close enough to require action, far enough to allow it.

This was the output that converted the problem from reactive to preventable. The
organisation could now see not just which tables were currently suffering, but which
ones would be suffering next — and on what timeline.

---

### NonParameterizedPredicatesCausingPlanCacheBloom

This was a separate but compounding problem.

Entity Framework 6 was generating non-parameterized SQL for several high-frequency
queries — embedding literal values directly in the query string rather than using
parameters. Every unique value produced a unique query string. Every unique query string
required a new compilation. Every compilation was added to the plan cache.

This is the primary driver of the 50,000 plans-per-hour figure. A single EF query
pattern generating unique SQL for each row ID it processes can produce thousands of
distinct cache entries per hour. The entries are never reused, which means they
contribute to cache pressure without providing the reuse benefit that justifies caching.

The replica server at 100,000 plans per hour was running the same queries under read
workloads — reporting queries, background jobs, integrations — all generating ad-hoc
SQL at scale.

`NonParameterizedPredicatesCausingPlanCacheBloom` identifies which queries are doing
this by flagging plans where the seek or filter predicates contain literal values rather
than parameter references. The column names in those predicates identify the EF query
patterns responsible.

---

### IndexSeekIsEffectivelyAFullScan / HighPercentageOfRowsReadFromIndex

Two related hypotheses for catching plans where the optimizer's row estimates were
wrong in ways that didn't necessarily show up as root-only seeks.

`HighPercentageOfRowsReadFromIndex` flags plans where the ratio of rows read to rows
returned is anomalously high — the index seek is touching far more rows than it
returns, indicating a poorly selective predicate or a missing covering column.

`IndexSeekIsEffectivelyAFullScan` is the degenerate case: the seek reads essentially
every row belonging to that tenant within the index. The query is structured as a seek
but behaving as a scan.

Both were added after the root-only seek problem was established — they catch the cases
where the predicate is technically present but still not selective enough.

#### The EF split query finding

Before the SARGability discovery, `HighPercentageOfRowsReadFromIndex` surfaced a
different problem entirely.

Serverless functions that should have completed in seconds were taking hours. The
individual queries looked clean — correct predicates, well-designed indexes. But the
hypothesis was flagging them at `Threshold.Over`: the query was reading *more rows than
the tenant actually had records*.

That shouldn't be physically possible from a single well-structured query. It pointed
to Entity Framework's split query feature.

EF Core's split query mode breaks a query with multiple collection navigations into
separate SQL statements to avoid cartesian explosion:

```sql
-- Query 1: fetch the main records
SELECT * FROM MainTable WHERE ClientId = @ClientId AND DeletedUtc < @UtcNow

-- Query 2: fetch all related records for those main records
SELECT * FROM RelatedTable WHERE MainTableId IN ( ... all IDs from Query 1 ... )
```

For a large tenant, Query 1 might return 500,000 rows. Query 2 then fetches every
related record across all 500,000 of them. If the related table has 3 rows per main
record, Query 2 reads 1.5 million rows. If there are three collection navigations,
EF issues three separate queries of similar scale. The total rows read across all
sub-queries easily exceeds the tenant's record count — which is exactly what
`Threshold.Over` catches.

The indexes were correct. The predicates were correct. The problem was that EF was
issuing multiple large queries where one targeted query would have done the work. The
serverless functions weren't slow because of bad SQL — they were slow because EF was
reading the same tenant's data three or four times over in separate roundtrips, each
one pulling hundreds of thousands of rows.

`HighPercentageOfRowsReadFromIndex` measures this at two levels: the percentage of the
tenant's own records read (`RootPercentage`), and the percentage of the whole table
read (`WholeTablePercentage`). `Threshold.Over` on either dimension — reading more
rows than exist — is the signal that something structural is wrong, not just a missing
index.

---

#### The unexpected finding: SARGability at scale

The most surprising result from these two hypotheses was what it revealed about index
design across the codebase.

The team had a consistent pattern: soft-delete timestamps (`DeletedUtc`) placed high
in index key columns, typically second after `ClientId`. The queries filtering against
these indexes almost universally looked like:

```sql
WHERE ClientId = @ClientId AND DeletedUtc < @UtcNow -- and any additional columns
```

The problem is that `DeletedUtc < @UtcNow` is a range predicate. SQL Server can only
use index key columns to the *left* of a range predicate as seek predicates. Everything
to the right becomes a residual predicate — applied row-by-row after the seek, not used
to narrow it. An index defined as `(ClientId, DeletedUtc, StatusColumn, OtherColumn)`
was providing exactly two useful seek columns: `ClientId` and `DeletedUtc`. The rest
were invisible to the optimizer.

Since soft deletes existed on virtually every significant table, and `DeletedUtc` was
in virtually every index, this pattern was nearly universal. The seeks were reading
entire tenant partitions filtered only by "not deleted yet" — which on a healthy system
is almost all rows — and then applying the remaining predicates as scans over that
result set.

The team kept adding indexes with more columns to fix slow queries, which did nothing
because the sargability problem was structural. The fix was to move range predicate
columns to the end of the key sequence or into `INCLUDE` columns.

#### How the pattern spread

This wasn't negligence — it was the normal mechanics of a full-stack team without a
dedicated data layer specialist. The team had one developer working at this depth. For
everyone else, the pattern spread the way most patterns spread: copy-paste from
existing code, and verbal advice from senior developers who knew enough to say "don't
forget to include the soft-delete filter" but not necessarily to add "and make sure it
isn't blocking the rest of your seek predicates."

The advice was correct as far as it went. The problem was invisible at small scale —
the queries performed fine until the first large client arrived. At that point the
indexes that had looked healthy for years were suddenly scanning millions of rows per
query, across every affected table, continuously.

The scale at which this was happening across the codebase was the finding that drove
the training. It was not a few problem queries — it was the default pattern, inherited
honestly across hundreds of indexes.

---

### BucketAnalysis — Validating the Fix

One of the only durable solutions to parameter sniffing in a shared-tenant database is
forcing SQL Server to compile separate plans per tenant size class. You cannot easily
have separate plan caches per tenant, but you can make queries *look different* to the
query optimizer depending on which size bucket the tenant falls into.

The approach used EF query tagging to append a tautological subquery expression to the
WHERE clause based on client size:

```sql
-- small tenant query
WHERE ClientId = @ClientId AND DeletedUtc < @UtcNow
-- AND (nothing added)

-- large tenant query  
WHERE ClientId = @ClientId AND DeletedUtc < @UtcNow
AND 4 = (SELECT 4) -- forces a unique query hash for this bucket
```

`4 = (SELECT 4)` is always true and never affects results. What it does is change the
query hash — SQL Server sees it as a distinct query and compiles a separate plan for
it. Large tenants get a plan compiled against large data. Small tenants get a plan
compiled against small data. The two plans never cross-contaminate each other's cache
entries.

The bucket size was embedded as a tag in the query text via EF:

```sql
/* Bucketed by BucketByQueryTagAndWhitelist to value large */
```

or as a query comment:

```sql
-- query_bucket_size=large;
```

`BucketAnalysis` extracted these tags from captured query text and correlated them
against the plan analysis results. For each plan, it recorded: which bucket compiled
it, which root ID was parameterized, whether the resulting plan still had a root-only
index seek, and which tables were involved.

This was how we validated that the bucketing was actually working — that large-bucket
plans were being compiled correctly, that small-bucket plans weren't leaking into large
tenant executions, and that the boundary conditions between buckets were in the right
place. A bucket that still showed root-only seek behavior was a signal that either the
threshold was wrong or the tag wasn't being applied consistently.

---

### RisingParameterSniffingIncreasesRatesForBadQueryPlans *(not implemented)*

The next hypothesis that never got built.

The intent was to correlate parameter sniffing rates across sessions captured over
time — not just "how many sniffed plans exist in this session" but "is the rate
increasing, and does an increase in sniffed plans predict a subsequent increase in
root-only seek plan reuse?"

The hypothesis mattered because the organization's position, even after accepting the
diagnosis, was that the problem was episodic — triggered by specific events like Monday
morning cache resets. The unbuilt hypothesis would have shown the trend between events:
that sniffing rates were rising in the background continuously, and that a threshold
crossing was what made each incident visible, not a discrete external trigger.

The engagement ended before this was built. The stub is in the codebase as a marker.

---

## What the Evidence Changed

The organization had been treating each performance incident as an isolated event and
responding with index additions. The hypothesis pipeline showed:

1. The volume was structural, not episodic — 50,000 compilations per hour is a
   baseline condition, not a symptom of specific queries
2. The index additions were increasing plan cache churn, not reducing it
3. The root cause was a small number of EF query patterns generating non-parameterized
   SQL and root-only seeks at high frequency
4. The largest tenant was the systematic victim of plans compiled against data two
   orders of magnitude smaller

The MCP server was the last component added — built so that the diagnostic capability
could survive the engagement and be used by people who hadn't spent 18 months reading
query plans. The intent was that Claude could query the plan database directly, find
patterns across sessions, and surface findings without requiring SQL expertise or
familiarity with the SHOWPLAN_XML format.

It was untested at the end of the engagement. The wiring is complete. Someone needs to
close the loop.
