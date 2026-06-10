namespace Encased.Access.Sqlite.CommonQueries;

public class ExistenceCheck
{
    public static string Query = $"SELECT name FROM sqlite_master WHERE type='table'";

    public required string Name { get; init; }
}