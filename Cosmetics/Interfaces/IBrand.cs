using Cosmetics.DTO.Brand;
using Cosmetics.Models;

namespace Cosmetics.Interfaces
{
    public interface IBrand
    {
        Task<List<Brand>> GetAllAsync();
        Task<Brand?> GetByIdAsync(Guid id);
        Task<Brand?> DeleteByIdAsync(Guid id);
        Task<Brand> CreateAsync(Brand brandModel);
        Task<Brand?> UpdateAsync(Guid id, UpdateBrandDTO brandDTO);
    }
}
