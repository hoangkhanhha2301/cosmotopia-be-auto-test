using Cosmetics.DTO.Product;
using Cosmetics.Helpers;
using Cosmetics.Models;

namespace Cosmetics.Interfaces
{
    public interface IProduct
    {
        Task<List<Product>> GetAllAsync(QueryObject query);
        Task<Product?> GetByIdAsync(Guid id);
        Task<Product> CreateAsync(Product productModel);
        Task<Product?> DeleteAsync(Guid id);
        Task<Product?> UpdateAsync(Guid id, UpdateProductDTO productDTO);
        Task<bool> CategoryExist(Guid categoryId);
        Task<bool> BranchExist(Guid branchId);
    }
}
