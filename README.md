# QueryPlan.Analyzer

Automated query plan hypothesis testing. Built in an afternoon to stop staring at XML.

It started as a single hypothesis check against a `.sqlplan` file. Every time a new
incident pattern repeated itself, a new hypothesis got added. By the end of the
engagement it had grown into a sampler, a multi-hypothesis pipeline, a table statistics
correlator, and the beginning of an MCP server so Claude could query the plan database
directly — because the diagnostic knowledge had to outlast the person who built it.

---

## How it works

Plans enter the pipeline from one of two sources: sampled live from a SQL Server plan
cache, or loaded from `.sqlplan` / `.xml` files on disk. Each plan is deserialized from
`SHOWPLAN_XML` format and run through a chain of analysis steps. Results are stored in
a local SQLite database and can be queried interactively via the MCP server or
programmatically through the CLI tools.

**Why two databases?** SQL Server's `SHOWPLAN_XML` is verbose. At 50,000 plans per
hour, uncompressed storage is gigabytes of XML. The sampler stores plans using SQL
Server's `COMPRESS()` function (GZip-compressed UTF-16 LE) into a compressed SQLite
database. The analyzer decompresses on demand into a separate uncompressed database
for fast querying and hypothesis evaluation. You only pay the decompression cost once.

```
SQL Server plan cache                    Disk (.sqlplan files)
        │                                         │
        ▼                                         ▼
QueryPlan.Sampler.CLI              QueryPlan.Analyzer.CLI
(compressed SQLite)                 (uncompressed SQLite)
        │                                         │
        └──────────────────┬──────────────────────┘
                           ▼
              QueryPlan.Analyzer.MCP
              (Claude queries plan DB)
```

---

## Tools

| Tool | What it does |
|---|---|
| `QueryPlan.Sampler.CLI` | Samples the live SQL Server plan cache and stores plans in a compressed SQLite database |
| `QueryPlan.Analyzer.CLI` | Diagnostic workbench — loads `.sqlplan` files from a folder or live-scrapes plans, stores them in an uncompressed SQLite database for analysis |
| `AppStatistics.CLI` | Fetches per-tenant table size data from SQL Server, used by hypotheses that need to distinguish large from small tenants |
| `Query.Auditor.CLI` | Grabs actual execution plans from SQL Server in real time and checks whether they will break for the largest tenant |
| `QueryPlan.Analyzer.MCP` | MCP server — exposes sampled plan sessions to Claude via the Model Context Protocol |

### A note on the CLI tools

The CLI entry points are diagnostic workbenches, not a polished pipeline. The
commented-out code throughout is intentional — it's the part you tune to match the
morning's specific incident before you run it. The sampler grabs from the plan cache;
the analyzer can feed from a folder of captured files or be pointed directly at a live
server. Which path you use depends on what you're investigating and how much time you
have before standup.

---

## Prerequisites

Set these environment variables before running any tool. See the
[root README](../README.md) for Podman and SQL Server setup.

| Variable | Used by | Purpose |
|---|---|---|
| `QPA_PRIMARY_CONNECTION` | Sampler, Auditor | Primary SQL Server connection string |
| `QPA_REPLICA_CONNECTION` | Sampler | Read-only replica connection string |
| `QPA_REPORTING_CONNECTION` | Auditor | Reporting database connection string |
| `QPA_UNCOMPRESSED_DB_PATH` | Analyzer CLI, MCP | Path to the uncompressed SQLite database |
| `QPA_COMPRESSED_DB_PATH` | Sampler | Path to the compressed SQLite database |
| `QPA_UNCOMPRESSED_FOLDER` | Analyzer CLI, Auditor | Folder containing `.sqlplan` / `.xml` files |

---

## Hypothesis reference

Hypotheses are the core of the pipeline. Each one receives the analysis results for
every plan in a session and emits a summary. They compose — run one or all of them
against the same session.

| Hypothesis | What it detects |
|---|---|
| `BasicStatistics` | Counts parameter-sniffed plans, root-only seek predicate plans, and non-parameterized predicate plans across the session — the opening summary for any investigation |
| `WillBreakForBiggestClient` | Plans whose index seeks use only the tenant root ID — will perform catastrophically when reused by the largest tenant |
| `TopQueriesResponsibleForBiggestClientRisk` | Ranks tables and root IDs by how often they appear in risky plans — identifies the hot spots |
| `LargeClientPlanAdoptedBySmallClient` | Detects parameter-sniffed plans compiled against a small tenant's data that will be catastrophically wrong when adopted by a large tenant |
| `SmallClientParameterSniffingIsRising` | Tracks which tenant root IDs are being compiled into cached plans — identifies concentration of small-tenant compilations |
| `NonParameterizedPredicatesCausingPlanCacheBloom` | Literal values embedded directly in WHERE clause predicates — each unique value compiles a new plan, bloating the cache |
| `IndexSeekIsEffectivelyAFullScan` | Index seeks that degenerate into full scans due to predicate structure |
| `HighPercentageOfRowsReadFromIndex` | Plans where estimated vs. actual rows-read ratios indicate the optimizer was badly wrong |
| `RisingParameterSniffingIncreasesRatesForBadQueryPlans` | **Not yet implemented.** Intended to correlate rising sniffing rates with bad plan adoption over time across sessions |
| `BucketAnalysis` | Internal utility — correlates plan analysis results with a tenant size bucket tag (`query_bucket_size=`) embedded in query text comments |

Hypotheses that require tenant size data (`LargeClientPlanAdoptedBySmallClient`,
`TopQueriesResponsibleForBiggestClientRisk`) depend on `AppStatistics.CLI` having
populated the table size catalog first.

---

## MCP server

`QueryPlan.Analyzer.MCP` exposes two tools to Claude:

| Tool | Description |
|---|---|
| `get_sessions` | Lists all sampled sessions with timestamps — find a session by date before analyzing |
| `analyze_session` | Runs the specified hypotheses against all plans in a session and returns a structured summary |

When analyzing hundreds of captured plans across multiple incidents, the MCP server
lets Claude find patterns across sessions without manual SQL — asking questions like
"which sessions from last Tuesday had root-only seek predicates on the reporting table?"

### Wiring it up

**Claude Desktop** — add to `claude_desktop_config.json`
(Windows: `%APPDATA%\Claude\claude_desktop_config.json`,
macOS: `~/Library/Application Support/Claude/claude_desktop_config.json`):

```json
{
  "mcpServers": {
    "queryplan-analyzer": {
      "command": "dotnet",
      "args": ["run", "--project", "path/to/QueryPlan.Analyzer.MCP"],
      "env": {
        "QPA_UNCOMPRESSED_DB_PATH": "/path/to/query-plans-uncompressed.db"
      }
    }
  }
}
```

**Claude Code** — add to `.mcp.json` in your project root:

```json
{
  "mcpServers": {
    "queryplan-analyzer": {
      "command": "dotnet",
      "args": ["run", "--project", "path/to/QueryPlan.Analyzer.MCP"],
      "env": {
        "QPA_UNCOMPRESSED_DB_PATH": "/path/to/query-plans-uncompressed.db"
      }
    }
  }
}
```

> **Note:** The MCP server was the last component built before the end of the
> engagement and has not been validated end-to-end against a live Claude session. The
> wiring is in place — it needs someone to close the loop. Contributions welcome.

---

## Building

```bash
dotnet build QueryPlan.Analyzer.sln
```

Requires .NET 10. All dependencies are on NuGet — no local package sources.
