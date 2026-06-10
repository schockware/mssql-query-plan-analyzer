using AppStatistics.Contracts.TableSizes;
using Encased.Access;

namespace AppStatistics.Access.TableSizes;

public class CachedTableStatisticsAccess(IDbAccess access)
    : ITableStatisticsAccess
{
    public async Task<IEnumerable<TableSize>> GetAllTableSizes()
    {
        var repo = access.GetRepository<TableSize>();
        return await repo.GetAsync(_ => true);
    }

    public async Task<IEnumerable<RootTableSize>> GetAllRootTableSizes()
    {
        var repo = access.GetRepository<RootTableSize>();
        return await repo.GetAsync(_ => true);
    }

    public async Task<List<TableSize>> Refresh(Func<Task<IEnumerable<TableSize>>> func)
    {
        await ArchiveTableSize();

        var repo = access.GetRepository<TableSize>();
        var latest = (await func()).ToList();
        await repo.SaveBulkAsync(latest);
        return latest;
    }

    public async Task<List<RootTableSize>> Refresh(Func<Task<IEnumerable<RootTableSize>>> func)
    {
        await ArchiveRootTableSize();

        var repo = access.GetRepository<RootTableSize>();
        var latest = (await func()).ToList();
        await repo.SaveBulkAsync(latest);
        return latest;
    }

    private async Task ArchiveTableSize()
    {
        var current = access.GetRepository<TableSize>();
        var archive = access.GetRepository<ArchivedTableSize>();
        var all = (await GetAllTableSizes()).ToList();
        var archivable = all.Select(s => new ArchivedTableSize(s));
        await archive.SaveBulkAsync(archivable);

        foreach (var size in all)
            await current.DeleteAsync(size);
    }

    private async Task ArchiveRootTableSize()
    {
        var current = access.GetRepository<RootTableSize>();
        var archive = access.GetRepository<ArchivedRootTableSize>();
        var all = (await GetAllRootTableSizes()).ToList();
        var archivable = all.Select(s => new ArchivedRootTableSize(s));
        await archive.SaveBulkAsync(archivable);

        foreach (var size in all)
            await current.DeleteAsync(size);
    }
}