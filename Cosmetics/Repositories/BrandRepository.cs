using Cosmetics.Interfaces;
using Cosmetics.Models;
using Microsoft.EntityFrameworkCore;

namespace Cosmetics.Repositories
{
    public class BrandRepository : GenericRepository<Brand>, IBrandRepository
    {
        private readonly ComedicShopDBContext _context;

        public BrandRepository(ComedicShopDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> brandHasProducts(Guid id)
        {
            return await _context.Products.AnyAsync(p => p.BrandId == id);
        }

        public async Task<bool> brandNameExist(string name)
        {
            return await _context.Brands.AnyAsync(b => b.Name.ToLower() == name.ToLower());
        }
    }
}
