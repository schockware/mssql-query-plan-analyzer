namespace Encased.Access.Sqlite;

[Serializable]
public class Config
{
    public required string DatabasePath { get; set; }
    public string ConnectionString => $"Data Source={DatabasePath}";
}