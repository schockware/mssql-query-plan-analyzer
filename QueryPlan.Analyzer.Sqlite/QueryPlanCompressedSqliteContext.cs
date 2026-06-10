using AppStatistics.Contracts.TableSizes;
using Encased.Access.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QueryPlan.Contracts;

namespace QueryPlan.Analyzer.Sqlite;

public class QueryPlanCompressedSqliteContext(IOptions<Config> options) : SqliteContext<QueryPlanCompressedSqliteContext>(options)
{
    public DbSet<QueryPlanRecordCompressed> QueryPlanRecordCompressed { get; set; }
    public DbSet<QueryTextCompressed> QueryTextCompressed { get; set; }
    public DbSet<SampleSession> SampleSession { get; set; }
    
    public DbSet<RootTableSize>  RootTableSize { get; set; }
    public DbSet<ArchivedRootTableSize> ArchivedRootTableSize { get; set; }
    public DbSet<TableSize> TableSize { get; set; }
    public DbSet<ArchivedTableSize> ArchivedTableSize { get; set; }
}