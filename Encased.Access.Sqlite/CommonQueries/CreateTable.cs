using System.Reflection;
using Encased.Contracts;

namespace Encased.Access.Sqlite.CommonQueries;

/// <summary>
/// For now, we want to avoid managing the Sqlite tables using migrations.
/// The main reason is there were some quirks early on that were causing some
/// of the fields to fail to bind when querying.
/// </summary>
public class CreateTable<T>
{
    private static PropertyInfo[] _tableProps = typeof(T).GetProperties();
    public static string TableName = typeof(T).Name;
    public static string Query = $"CREATE TABLE IF NOT EXISTS {TableName} ({GetColumnDefinitions()})";

    private static string GetColumnDefinitions()
        => string.Join(",", _tableProps.Select(GetColumnDefinition));
    private static string GetColumnDefinition(PropertyInfo prop)
    {
        if (prop.Name == nameof(IGuidEntity.Id))
            return "Id TEXT PRIMARY KEY UNIQUE";

        if (prop.PropertyType == typeof(bool) || prop.PropertyType == typeof(byte) ||
            prop.PropertyType == typeof(short) || prop.PropertyType == typeof(int) || prop.PropertyType == typeof(long))
            return $"{prop.Name} INTEGER";
        if (prop.PropertyType == typeof(float) || prop.PropertyType == typeof(double) ||
            prop.PropertyType == typeof(decimal))
            return $"{prop.Name} REAL";
        if (prop.PropertyType == typeof(byte[]))
            return $"{prop.Name} BLOB";

        return $"{prop.Name} TEXT";
    }
}