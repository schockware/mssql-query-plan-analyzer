namespace Encased.Access.MsSql;

public class Config
{
    public string PrimaryConnectionString { get; set; } = string.Empty;
    public string ReplicaConnectionString { get; set; } = string.Empty;
    public string ReportingConnectionString { get; set; } = string.Empty;
}