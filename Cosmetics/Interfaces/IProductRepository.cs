using Cosmetics.Models;

namespace Cosmetics.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<bool> CategoryExist(Guid categoryId);
        Task<bool> BranchExist(Guid branchId);
    }
}
