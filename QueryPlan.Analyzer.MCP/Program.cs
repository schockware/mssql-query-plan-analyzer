using Encased.Access;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QueryPlan.Analyzer.MCP.Tools;
using QueryPlan.Common.StartUps;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddUncompressedAccessors(config =>
    config.DatabasePath = Environment.GetEnvironmentVariable("QPA_UNCOMPRESSED_DB_PATH")
        ?? throw new InvalidOperationException("QPA_UNCOMPRESSED_DB_PATH environment variable is not set."));

builder.Services.AddSingleton<GetSessionsTool>();
builder.Services.AddSingleton<AnalyzeSessionTool>();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly(typeof(Program).Assembly);

var host = builder.Build();

await host.Services.GetRequiredService<IDbAccess>().Initialize();

await host.RunAsync();
