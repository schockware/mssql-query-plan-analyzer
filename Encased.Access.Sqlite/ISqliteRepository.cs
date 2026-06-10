using Encased.Contracts;

namespace Encased.Access.Sqlite;

public interface ISqliteRepository
{
    string TableName { get; }
    Task EnsureTableExists();
}
public interface ISqliteRepository<T> : IGuidEntityRepository<T>, ISqliteRepository
    where T : IGuidEntity
{

}