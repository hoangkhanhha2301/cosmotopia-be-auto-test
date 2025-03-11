using Cosmetics.DTO.Product;
using Cosmetics.Helpers;
using Cosmetics.Interfaces;
using Cosmetics.Models;
using Microsoft.EntityFrameworkCore;

namespace Cosmetics.Repositories
{
    public class ProductRepository : IProduct
    {
        private readonly ComedicShopDBContext _context;

        public ProductRepository(ComedicShopDBContext context)
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

        public async Task<Product> CreateAsync(Product productModel)
        {
            await _context.Products.AddAsync(productModel);
            await _context.SaveChangesAsync();
            return productModel;
        }

        public async Task<Product?> DeleteAsync(Guid id)
        {
            var productId = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == id);
            if(productId == null)
            {
                return null;
            }

            _context.Products.Remove(productId);
            await _context.SaveChangesAsync();
            return productId;
        }

        public async Task<List<Product>> GetAllAsync(QueryObject query)
        {
            var queryObject = _context.Products.Include(b => b.Brand).Include(c => c.Category).AsQueryable();

            #region Pagination
            var skipNumber = (query.pageNumber - 1) * query.pageSize;
            #endregion

            return await queryObject.Skip(skipNumber).Take(query.pageSize).ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            return await _context.Products.FirstOrDefaultAsync(p => p.ProductId == id);
        }

        public async Task<Product?> UpdateAsync(Guid id, UpdateProductDTO productDTO)
        {
            var existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == id);
            if (existingProduct == null)
            {
                return null;
            }

            existingProduct.Name = productDTO.Name;
            existingProduct.Description = productDTO.Description;
            existingProduct.Price = productDTO.Price;
            existingProduct.StockQuantity = productDTO.StockQuantity;
            existingProduct.ImageUrls = productDTO.ImageUrls;
            existingProduct.CommissionRate = productDTO.CommissionRate;
            DateTime currentTime = DateTime.Now;
            existingProduct.UpdatedAt = currentTime;
            existingProduct.IsActive = productDTO.IsActive;

            await _context.SaveChangesAsync();
            return existingProduct;
        }
    }
}
