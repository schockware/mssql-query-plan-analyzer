// See https://aka.ms/new-console-template for more information

using Encased.Access.MsSql;
using Microsoft.Extensions.DependencyInjection;
using Query.Auditor;
using QueryPlan.Common.StartUps;

Console.WriteLine("Hello, World!");

var provider = await new ProgramBuilder(new ProgramBuilder.Options()
{
    StorageOption = ProgramBuilder.StorageOption.KeepCompressed,
    Add = (services) =>
    {
        services.AddSingleton<RawQueryAccessor>();
        services.AddSingleton<EstimatedExecutionPlanAccessor>();
        services.AddSingleton<PlanGrabber>();
    }
    
}).Build();

var planGrabber =  provider.GetRequiredService<PlanGrabber>();
await planGrabber.GetIt();