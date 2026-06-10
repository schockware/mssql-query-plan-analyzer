using AppStatistics.Contracts.TableSizes;
using Encased.Access.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QueryPlan.Contracts;

namespace QueryPlan.Analyzer.Sqlite;

public class QueryPlanUncompressedSqliteContext(IOptions<Config> options) : SqliteContext<QueryPlanUncompressedSqliteContext>(options)
{
    public DbSet<QueryPlanRecord> QueryPlanRecord { get; set; }
    public DbSet<QueryText> QueryText { get; set; }
    public DbSet<SampleSession> SampleSession { get; set; }
    
    public DbSet<RootTableSize>  RootTableSize { get; set; }
    public DbSet<ArchivedRootTableSize> ArchivedRootTableSize { get; set; }
    public DbSet<TableSize> TableSize { get; set; }
    public DbSet<ArchivedTableSize> ArchivedTableSize { get; set; }
}