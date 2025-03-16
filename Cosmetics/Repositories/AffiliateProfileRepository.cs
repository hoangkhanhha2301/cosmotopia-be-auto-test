using Cosmetics.Interfaces;
using Cosmetics.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Cosmetics.Repositories
{
    public class AffiliateProfileRepository : GenericRepository<AffiliateProfile>, IAffiliateProfileRepository
    {
        public AffiliateProfileRepository(ComedicShopDBContext context) : base(context) { }

        public async Task<AffiliateProfile> FirstOrDefaultAsync(Expression<Func<AffiliateProfile, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }
    }
}
