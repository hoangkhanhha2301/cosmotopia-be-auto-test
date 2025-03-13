using Cosmetics.Interfaces;
using Cosmetics.Models;
using Microsoft.EntityFrameworkCore;

namespace Cosmetics.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        private readonly ComedicShopDBContext _context;

        public ProductRepository(ComedicShopDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> BranchExist(Guid branchId)
        {
            return await _context.Brands.AnyAsync(b => b.BrandId == branchId);
        }

        public async Task<bool> CategoryExist(Guid categoryId)
        {
            return await _context.Categories.AnyAsync(c => c.CategoryId == categoryId);
        }
    }
}
