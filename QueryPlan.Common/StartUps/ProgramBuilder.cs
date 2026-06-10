using AppStatistics.Access.TableSizes;
using AppStatistics.Contracts.TableSizes;
using AppStatistics.TableSizes;
using Encased.Access;
using Encased.Access.System;
using Microsoft.Extensions.DependencyInjection;
using QueryPlan.Sampler;

namespace QueryPlan.Common.StartUps;

[Obsolete("We need a common startup for the analysis and the sampler")]
public class ProgramBuilder(ProgramBuilder.Options options)
{
    public async Task<IServiceProvider> Build()
    {
        var services = new ServiceCollection();
        if (options.StorageOption == StorageOption.Uncompress)
        {
            services.AddUncompressedAccessors(config =>
                config.DatabasePath = Environment.GetEnvironmentVariable("QPA_UNCOMPRESSED_DB_PATH")
                    ?? throw new InvalidOperationException("QPA_UNCOMPRESSED_DB_PATH environment variable is not set."));
            services.AddSingleton<ICachePlanSampler, UncompressedCachePlanSampler>();
        }
        else
        {
            services.AddCompressedAccessors(config =>
                config.DatabasePath = Environment.GetEnvironmentVariable("QPA_COMPRESSED_DB_PATH")
                    ?? throw new InvalidOperationException("QPA_COMPRESSED_DB_PATH environment variable is not set."));
            services.AddSingleton<ICachePlanSampler, CompressedCachePlanSampler>();
        }

        services.AddSingleton<CachedTableStatisticsAccess>();
        services.AddSingleton<SourceTableStatisticsAccess>();
        services.AddSingleton<ITableStatisticsAccess, TieredTableStatisticsAccess>();

        services.Configure<FolderAccess.Config>(config =>
        {
            config.Root = Environment.GetEnvironmentVariable("QPA_UNCOMPRESSED_FOLDER")
                ?? throw new InvalidOperationException("QPA_UNCOMPRESSED_FOLDER environment variable is not set.");
        });
        services.AddSingleton<FolderAccess>();

        services.AddSqlSources(config =>
        {
            config.PrimaryConnectionString =
                Environment.GetEnvironmentVariable("QPA_PRIMARY_CONNECTION")
                ?? throw new InvalidOperationException("QPA_PRIMARY_CONNECTION environment variable is not set.");
            config.ReplicaConnectionString =
                Environment.GetEnvironmentVariable("QPA_REPLICA_CONNECTION")
                ?? throw new InvalidOperationException("QPA_REPLICA_CONNECTION environment variable is not set.");
            config.ReportingConnectionString =
                Environment.GetEnvironmentVariable("QPA_REPORTING_CONNECTION")
                ?? throw new InvalidOperationException("QPA_REPORTING_CONNECTION environment variable is not set.");
        });

        options.Add(services);

        var provider = services.BuildServiceProvider();
        await provider.GetRequiredService<IDbAccess>().Initialize();

        var tableAccess = provider.GetRequiredService<CachedTableStatisticsAccess>();
        var rootSizes = await tableAccess.GetAllRootTableSizes();
        var sizes = await tableAccess.GetAllTableSizes();
        services.AddSingleton<ITableSizeCatalog>(new TableSizeCatalog(rootSizes, sizes));

        return services.BuildServiceProvider();
    }

    public class Options
    {
        public StorageOption StorageOption { get; set; }
        public Action<IServiceCollection> Add { get; set; } = (_) => { }; //NoOp default
    }

    public enum StorageOption
    {
        KeepCompressed,
        Uncompress,
    }
}