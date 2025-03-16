using Cosmetics.Models;
using System.Linq.Expressions;

namespace Cosmetics.Interfaces
{
    public interface IAffiliateProfileRepository : IGenericRepository<AffiliateProfile>
    {
        // Add specific methods if needed beyond IGenericRepository
        Task<AffiliateProfile> FirstOrDefaultAsync(Expression<Func<AffiliateProfile, bool>> predicate);
    }
}
