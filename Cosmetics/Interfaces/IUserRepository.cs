using Cosmetics.Models;
using System.Linq.Expressions;

namespace Cosmetics.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User> FirstOrDefaultAsync(Expression<Func<User, bool>> predicate);
    }
}
