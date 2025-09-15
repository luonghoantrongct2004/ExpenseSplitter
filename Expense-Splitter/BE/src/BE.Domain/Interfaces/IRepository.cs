using BE.Domain.Entities;
using System.Linq.Expressions;

namespace BE.Domain.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id);

    Task<IEnumerable<T>> GetAllAsync();

    Task<T> AddAsync(T entity);

    Task UpdateAsync(T entity);

    Task DeleteAsync(T entity);

    Task<bool> ExistsAsync(Guid id);

    // Specification pattern methods
    Task<T?> FirstOrDefaultAsync(ISpecification<T> spec);

    Task<IEnumerable<T>> ListAsync(ISpecification<T> spec);

    Task<int> CountAsync(ISpecification<T> spec);

    Task<bool> AnyAsync(ISpecification<T> spec);

    // Additional useful methods
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);

    Task UpdateRangeAsync(IEnumerable<T> entities);

    Task DeleteRangeAsync(IEnumerable<T> entities);

    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
}