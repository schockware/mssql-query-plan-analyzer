using System.Reflection;
using Encased.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Encased.Access.Sqlite;

public abstract class SqliteContext<T>(IOptions<Config> options) : DbContext
    where T : SqliteContext<T>
{
    private static readonly Lazy<Dictionary<Type, PropertyInfo>> _dbSetProperties = new(() =>
        typeof(T).GetProperties()
            .Where(prop => prop.PropertyType.Name == typeof(DbSet<>).Name)
            .ToDictionary(prop => prop.PropertyType.GetGenericArguments().First(), prop => prop));

    private static Dictionary<Type, PropertyInfo> DbSetProperties => _dbSetProperties.Value;


    public string ConnectionString => options.Value.ConnectionString;

    public DbSet<T> With<T>() where T : Uuid7Entity
    {
        if (DbSetProperties.TryGetValue(typeof(T), out var propertyInfo))
            return propertyInfo.GetValue(this) as DbSet<T> ?? throw new Exception($"Could not cast as DbSet<{typeof(T).Name}>");
        throw new Exception($"Could not find as DbSet<{typeof(T).Name}>");
    }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite(options.Value.ConnectionString);

}