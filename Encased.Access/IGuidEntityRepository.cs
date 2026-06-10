using System.Linq.Expressions;
using Encased.Contracts;

namespace Encased.Access;

public interface IGuidEntityRepository<T>
    where T : IGuidEntity
{
    Task<T?> GetAsync(Guid id);
    Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> predicate);
    Task DeleteAsync(Guid id);
    Task DeleteAsync(T item);
    Task SaveAsync(T entity);
    Task SaveBulkAsync(IEnumerable<T> entities);
}