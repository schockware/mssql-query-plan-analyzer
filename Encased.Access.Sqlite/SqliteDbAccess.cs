using Dapper;
using Encased.Access.Sqlite.CommonQueries;
using Encased.Contracts;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace Encased.Access.Sqlite;

public class SqliteDbAccess(IOptions<Config> options, IEnumerable<ISqliteRepository> repositories)
    : IDbAccess
{
    private readonly List<ISqliteRepository> _repositories = repositories.ToList();
    private readonly Dictionary<Type, ISqliteRepository> _repositoryIndex = repositories
        .Where(r => r.GetType().GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IGuidEntityRepository<>)))
        .ToDictionary(
            r => r.GetType().GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IGuidEntityRepository<>))
                .GetGenericArguments()[0],
            r => r);

    public async Task Initialize()
    {
        await EnsureDbExists();
        await EnsureTablesExist();
    }

    public IGuidEntityRepository<T> GetRepository<T>()
        where T : IGuidEntity
    {
        if (_repositoryIndex.TryGetValue(typeof(T), out var repository) &&
            repository is IGuidEntityRepository<T> entityRepo)
            return entityRepo;

        throw new InvalidOperationException($"No repository registered for type '{typeof(T).Name}'.");
    }

    private async Task EnsureDbExists()
    {
        if (File.Exists(options.Value.DatabasePath))
            return;

        await using var conn = GetConnection();
        await conn.OpenAsync();
    }

    private SqliteConnection GetConnection()
        => new(options.Value.ConnectionString);

    private async Task EnsureTablesExist()
    {
        await using var conn = GetConnection();
        var tables = await conn.QueryAsync<ExistenceCheck>(ExistenceCheck.Query);
        var missingRepositories = _repositories.Where(x => tables.All(y => y.Name != x.TableName)).ToList();
        foreach (var missingRepository in missingRepositories)
            await missingRepository.EnsureTableExists();
    }
}
