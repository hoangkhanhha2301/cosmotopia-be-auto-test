using Cosmetics.DTO.Category;
using Cosmetics.Interfaces;
using Cosmetics.Models;
using Microsoft.EntityFrameworkCore;

namespace Cosmetics.Repositories
{
    public class CategoryRepository : ICategory
    {
        private readonly ComedicShopDBContext _context;

        public CategoryRepository(ComedicShopDBContext context)
        {
            _context = context;
        }

        public async Task<bool> CategoryHasProducts(Guid id)
        {
            return await _context.Products.AnyAsync(p => p.CategoryId == id);
        }

        public async Task<Category> CreateAsync(Category categoryModel)
        {
            await _context.AddAsync(categoryModel);
            await _context.SaveChangesAsync();
            return categoryModel;
        }

        public async Task<Category?> DeleteAsync(Guid id)
        {
            var categoryId = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == id);
            if (categoryId == null) 
            {
                return null; 
            }

            _context.Categories.Remove(categoryId);
            await _context.SaveChangesAsync();
            return categoryId;
        }

        public async Task<List<Category>> GetAllAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(Guid id)
        {
            return await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == id);
        }

        public async Task<Category?> UpdateAsync(Guid id, UpdateCategoryDTO categoryDTO)
        {
            var existingcategory = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == id);
            if (existingcategory == null)
            {
                return null;
            }

            existingcategory.Name = categoryDTO.Name;
            existingcategory.Description = categoryDTO.Description;

            await _context.SaveChangesAsync();
            return existingcategory;
        }
    }
}
