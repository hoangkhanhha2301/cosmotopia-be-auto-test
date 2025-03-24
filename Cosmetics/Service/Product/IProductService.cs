using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cosmetics.Models;

public interface IProductService
{
    Task<Product> GetProductByIdAsync(Guid productId);
    Task<List<Product>> GetAllProductsAsync(int pageNumber = 1, int pageSize = 10);
    Task<List<Product>> GetProductsByCategoryAsync(Guid categoryId);
    Task<List<Product>> GetProductsByBrandAsync(Guid brandId);
    Task<bool> IsPremiumBrandAsync(Guid productId);
    Task<decimal> GetCommissionRateAsync(Guid productId);
    //Task<List<Product>> GetTopPerformingProductsAsync(Guid affiliateProfileId, int topCount = 5); // Sửa thành Guid
    Task<bool> UpdateStockQuantityAsync(Guid productId, int quantity);
}