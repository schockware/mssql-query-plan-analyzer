using Dapper;
using System.Linq.Expressions;
using Encased.Access.Sqlite.CommonQueries;
using Encased.Contracts;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Encased.Access.Sqlite;

public class Uuid7Repository<T, TContext>(SqliteContext<TContext> context) : ISqliteRepository<T>
    where T : Uuid7Entity
    where TContext : SqliteContext<TContext>
{
    public string TableName { get; } = typeof(T).Name;
    private readonly DbSet<T> table = context.With<T>();

    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetAsync(id);
        if (entity == null)
            return;
        await DeleteAsync(entity);
    }

    public async Task DeleteAsync(T item)
    {
        table.Remove(item);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// For now, we want to avoid managing the Sqlite tables using migrations.
    /// The main reason is there were some quirks early on that were causing some
    /// of the fields to fail to bind when querying.
    /// </summary>
    public async Task EnsureTableExists()
    {
        await using var conn = GetConnection();
        await conn.ExecuteAsync(CreateTable<T>.Query);
    }

    public async Task<T?> GetAsync(Guid id)
        => await table.FirstOrDefaultAsync(e => e.Id == id);

    public async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> predicate)
        => await table.Where(predicate).ToListAsync();

    public async Task SaveBulkAsync(IEnumerable<T> entities)
    {
        var pending = entities.ToList();
        var existingIds = await table.Select(e => e.Id).ToHashSetAsync();
        var index = 0;
        try
        {
            foreach (var entity in pending)
            {
                if (existingIds.Contains(entity.Id))
                    table.Update(entity);
                else
                    await table.AddAsync(entity);

                index++;

                if (index % 100 == 0)
                    await context.SaveChangesAsync();
            }

            await context.SaveChangesAsync();
        }
        catch (Exception)
        {
            context.ChangeTracker.Clear();
            throw;
        }
    }

    public async Task SaveAsync(T entity)
    {
        var existing = await GetAsync(entity.Id);
        if (existing == null)
            await table.AddAsync(entity);
        else
            table.Update(existing.CopyFrom(entity));
        await context.SaveChangesAsync();
    }

    private SqliteConnection GetConnection()
        => new(context.ConnectionString);
}