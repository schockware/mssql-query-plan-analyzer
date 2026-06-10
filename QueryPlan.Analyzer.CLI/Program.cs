using Encased.Access;
using Encased.Access.System;
using Microsoft.Extensions.DependencyInjection;
using QueryPlan.Common.StartUps;
using QueryPlan.Contracts;

var provider = await new ProgramBuilder(new ProgramBuilder.Options()
    { StorageOption = ProgramBuilder.StorageOption.Uncompress }).Build();

var folderAccess = provider.GetRequiredService<FolderAccess>();
var access = provider.GetRequiredService<IDbAccess>();
var queryTextRepo = access.GetRepository<QueryText>();
var queryPlanRepo = access.GetRepository<QueryPlanRecord>();
