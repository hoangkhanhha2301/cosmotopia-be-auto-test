using Cosmetics.DTO.Category;
using Cosmetics.Models;

namespace Cosmetics.Interfaces
{
    public interface ICategory
    {
        Task<List<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(Guid id);
        Task<Category?> UpdateAsync(Guid id, UpdateCategoryDTO categoryDTO);
        Task<Category?> DeleteAsync(Guid id);
        Task<Category> CreateAsync(Category categoryModel);
    }
}
