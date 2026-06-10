// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.DependencyInjection;
using QueryPlan.Common.StartUps;
using QueryPlan.Sampler;

Console.WriteLine("Hello, World!");

var builder = new ProgramBuilder(new ProgramBuilder.Options(){ StorageOption = ProgramBuilder.StorageOption.KeepCompressed });

var provider = await builder.Build();

var sampler = provider.GetRequiredService<ICachePlanSampler>();

await sampler.LoadSamples();