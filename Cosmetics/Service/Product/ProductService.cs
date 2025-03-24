using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cosmetics.Models;

public class ProductService : IProductService
{
    private readonly ComedicShopDBContext _context; // DbContext của bạn

    public ProductService(ComedicShopDBContext context)
    {
        _context = context;
    }

    public async Task<Product> GetProductByIdAsync(Guid productId)
    {
        return await _context.Products
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.ProductId == productId);
    }

    public async Task<List<Product>> GetAllProductsAsync(int pageNumber = 1, int pageSize = 10)
    {
        return await _context.Products
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<Product>> GetProductsByCategoryAsync(Guid categoryId)
    {
        return await _context.Products
            .Where(p => p.CategoryId == categoryId)
            .ToListAsync();
    }

    public async Task<List<Product>> GetProductsByBrandAsync(Guid brandId)
    {
        return await _context.Products
            .Where(p => p.BrandId == brandId)
            .ToListAsync();
    }

    public async Task<bool> IsPremiumBrandAsync(Guid productId)
    {
        var product = await _context.Products
            .Include(p => p.Brand)
            .FirstOrDefaultAsync(p => p.ProductId == productId);

        return product?.Brand?.IsPremium ?? false; // Xử lý nullable
    }

    public async Task<decimal> GetCommissionRateAsync(Guid productId)
    {
        var product = await GetProductByIdAsync(productId);
        if (product == null)
        {
            return 0m; // Trả về 0 nếu sản phẩm không tồn tại
        }

        var isPremium = await IsPremiumBrandAsync(productId);
        return isPremium ? 5.0m : 2.0m; // 5% cho Premium, 2% cho không Premium
    }

    //public async Task<List<Product>> GetTopPerformingProductsAsync(Guid affiliateProfileId, int topCount = 5)
    //{
    //    var topProducts = await _context.AffiliateCommissions
    //        .Where(ac => ac.AffiliateProfileId == affiliateProfileId)
    //        .Join(_context.OrderDetails,
    //            ac => ac.OrderDetailId, // Sửa thành OrderDetailId
    //            od => od.OrderDetailId,
    //            (ac, od) => new { ac, od })
    //        .GroupBy(x => x.od.ProductId)
    //        .Select(g => new
    //        {
    //            //ProductId = g.Key,
    //            //TotalRevenue = g.Sum(x => x.ac.CommissionAmount),
    //            //TotalClicks = _context.ClickTrackings
    //            //    .Count(ct => ct.AffiliateProfileId == affiliateProfileId && ct.ProductId == g.Key)
    //        })
    //        .OrderByDescending(x => x.TotalRevenue)
    //        .Take(topCount)
    //        .Join(_context.Products,
    //            x => x.ProductId,
    //            p => p.ProductId,
    //            (x, p) => p)
    //        .ToListAsync();

    //    return topProducts;
    //}

    public async Task<bool> UpdateStockQuantityAsync(Guid productId, int quantity)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null || product.StockQuantity == null || product.StockQuantity < quantity)
        {
            return false;
        }

        product.StockQuantity -= quantity;
        product.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();
        return true;
    }
}