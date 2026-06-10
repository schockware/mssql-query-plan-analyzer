// See https://aka.ms/new-console-template for more information

using AppStatistics.Access.TableSizes;
using Microsoft.Extensions.DependencyInjection;
using QueryPlan.Common.StartUps;

Console.WriteLine("Hello, World!");

var provider = await new ProgramBuilder(new ProgramBuilder.Options()
    { StorageOption = ProgramBuilder.StorageOption.KeepCompressed, Add = (services) =>
    {
        services.AddSingleton<CachedTableStatisticsAccess>();
        services.AddSingleton<SourceTableStatisticsAccess>();
        services.AddSingleton<ITableStatisticsAccess, TieredTableStatisticsAccess>();
    }}).Build();

var access = provider.GetRequiredService<ITableStatisticsAccess>();

await access.GetAllTableSizes();
await access.GetAllRootTableSizes();
