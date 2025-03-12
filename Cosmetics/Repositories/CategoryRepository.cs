using Cosmetics.DTO.Category;
using Cosmetics.Interfaces;
using Cosmetics.Models;
using Microsoft.EntityFrameworkCore;

namespace Cosmetics.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        private readonly ComedicShopDBContext _context;

        public CategoryRepository(ComedicShopDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> categoryHasProducts(Guid id)
        {
            return await _context.Products.AnyAsync(p => p.CategoryId == id);
        }

        public async Task<bool> categoryNameExist(string name)
        {
            return await _context.Categories.AnyAsync(c => c.Name.ToLower() == name.ToLower());
        }
    }
}
