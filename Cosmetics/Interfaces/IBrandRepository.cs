using Cosmetics.Models;

namespace Cosmetics.Interfaces
{
    public interface IBrandRepository : IGenericRepository<Brand>
    {
        Task<bool> brandHasProducts(Guid id);
        Task<bool> brandNameExist(string name);
    }
}
