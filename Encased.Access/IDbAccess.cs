using Encased.Contracts;

namespace Encased.Access;

public interface IDbAccess
{
    Task Initialize();
    IGuidEntityRepository<T> GetRepository<T>() 
        where T : IGuidEntity;
}