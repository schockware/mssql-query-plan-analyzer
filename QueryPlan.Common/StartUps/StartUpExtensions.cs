using AppStatistics.Contracts.TableSizes;
using Encased.Access;
using Encased.Access.MsSql;
using Encased.Access.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using QueryPlan.Analyzer.Sqlite;
using QueryPlan.Contracts;

namespace QueryPlan.Common.StartUps;

public static class StartUpExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSqlSources(Action<Encased.Access.MsSql.Config> config)
        {
            services.Configure(config);
            services.AddSingleton<CacheAccessor>();
        
            return services;
        }

        public IServiceCollection AddUncompressedAccessors(Action<Encased.Access.Sqlite.Config> config)
        {
            services.Configure(config);

            services.AddSingleton<SqliteContext<QueryPlanUncompressedSqliteContext>, QueryPlanUncompressedSqliteContext>();
            services.AddSingleton<ISqliteRepository, Uuid7Repository<QueryPlanRecord, QueryPlanUncompressedSqliteContext>>();
            services.AddSingleton<ISqliteRepository, Uuid7Repository<QueryText, QueryPlanUncompressedSqliteContext>>();
            services.AddSingleton<ISqliteRepository, Uuid7Repository<SampleSession, QueryPlanUncompressedSqliteContext>>();

            services.AddSingleton<ISqliteRepository, Uuid7Repository<RootTableSize, QueryPlanUncompressedSqliteContext>>();
            services.AddSingleton<ISqliteRepository, Uuid7Repository<TableSize, QueryPlanUncompressedSqliteContext>>();
            services.AddSingleton<ISqliteRepository, Uuid7Repository<ArchivedRootTableSize, QueryPlanUncompressedSqliteContext>>();
            
            services.AddSingleton<IDbAccess, SqliteDbAccess>();
        
            return services;
        }

        public IServiceCollection AddCompressedAccessors(Action<Encased.Access.Sqlite.Config> config)
        {
            services.Configure(config);

            services.AddSingleton<SqliteContext<QueryPlanCompressedSqliteContext>, QueryPlanCompressedSqliteContext>();
            services.AddSingleton<ISqliteRepository, Uuid7Repository<QueryPlanRecordCompressed, QueryPlanCompressedSqliteContext>>();
            services.AddSingleton<ISqliteRepository, Uuid7Repository<QueryTextCompressed, QueryPlanCompressedSqliteContext>>();
            services.AddSingleton<ISqliteRepository, Uuid7Repository<SampleSession, QueryPlanCompressedSqliteContext>>();

            services.AddSingleton<ISqliteRepository, Uuid7Repository<RootTableSize, QueryPlanCompressedSqliteContext>>();
            services.AddSingleton<ISqliteRepository, Uuid7Repository<TableSize, QueryPlanCompressedSqliteContext>>();
            services.AddSingleton<ISqliteRepository, Uuid7Repository<ArchivedRootTableSize, QueryPlanCompressedSqliteContext>>();
            services.AddSingleton<ISqliteRepository, Uuid7Repository<ArchivedTableSize, QueryPlanCompressedSqliteContext>>();
            
            services.AddSingleton<IDbAccess, SqliteDbAccess>();
        
            return services;
        }
    }
}