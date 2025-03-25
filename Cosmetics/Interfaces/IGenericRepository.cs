using Cosmetics.Models;
using System.Linq.Expressions;

namespace Cosmetics.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> predicate);
        Task AddRangeAsync(IEnumerable<T> entity);
        Task<T> GetByIdAsync<Tid>(Tid id);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        void Delete(T entity);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync(Expression<Func<T, bool>> filter = null);
        Task<IEnumerable<T>> GetAsync(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            int? page = null,
            int? pageSize = null,
            Func<IQueryable<T>, IQueryable<T>>[] includeOperations = null);
    }
}