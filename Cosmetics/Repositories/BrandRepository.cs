using Cosmetics.DTO.Brand;
using Cosmetics.Interfaces;
using Cosmetics.Models;
using Microsoft.EntityFrameworkCore;

namespace Cosmetics.Repositories
{
    public class BrandRepository : IBrand
    {
        private readonly ComedicShopDBContext _context;

        public BrandRepository(ComedicShopDBContext context)
        {
            _context = context;
        }

        public async Task<bool> BrandHasProducts(Guid id)
        {
            return await _context.Products.AnyAsync(p => p.BrandId == id);
        }

        public async Task<Brand> CreateAsync(Brand brandModel)
        {
            await _context.AddAsync(brandModel);
            await _context.SaveChangesAsync();
            return brandModel;
        }

        public async Task<Brand?> DeleteByIdAsync(Guid id)
        {
            var brandId = await _context.Brands.FirstOrDefaultAsync(b => b.BrandId == id);
            if (brandId == null)
            {
                return null;
            }

            _context.Brands.Remove(brandId);
            await _context.SaveChangesAsync();
            return brandId;
        }

        public async Task<List<Brand>> GetAllAsync()
        {
            return await _context.Brands.ToListAsync();
        }

        public async Task<Brand?> GetByIdAsync(Guid id)
        {
            return await _context.Brands.FirstOrDefaultAsync(b => b.BrandId == id);
        }

        public async Task<Brand?> UpdateAsync(Guid id, UpdateBrandDTO brandDTO)
        {
            var existingBrand = await _context.Brands.FirstOrDefaultAsync(b => b.BrandId == id);
            if (existingBrand == null)
            {
                return null;
            }

            existingBrand.Name = brandDTO.Name;
            existingBrand.IsPremium = brandDTO.IsPremium;

            await _context.SaveChangesAsync();
            return existingBrand;
        }
    }
}
