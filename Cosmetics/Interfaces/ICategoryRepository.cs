using Cosmetics.Models;

namespace Cosmetics.Interfaces
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<bool> categoryHasProducts(Guid id);
        Task<bool> categoryNameExist(string name);
    }
}
