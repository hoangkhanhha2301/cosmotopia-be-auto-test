using Cosmetics.DTO.Affiliate;
using Cosmetics.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cosmetics.Service.Affiliate
{
    public class AffiliateService : IAffiliateService
    {
        private readonly ComedicShopDBContext _context;

        public AffiliateService(ComedicShopDBContext context)
        {
            _context = context;
        }

        public async Task<User> GetUserById(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }



        public async Task<AffiliateProfile> GetAffiliateProfile(int userId)
        {
            return await _context.AffiliateProfiles
                .FirstOrDefaultAsync(a => a.UserId == userId);
        }

        public async Task<AffiliateIncomeDto> GetWeeklyIncome(Guid affiliateProfileId)
        {
            var profile = await _context.AffiliateProfiles.FindAsync(affiliateProfileId);
            return new AffiliateIncomeDto
            {
                TotalEarnings = profile?.TotalEarnings ?? 0,
                PendingAmount = profile?.PendingAmount ?? 0,
                WithdrawnAmount = profile?.WithdrawnAmount ?? 0,
                WeeklyClicks = await _context.ClickTrackings
                    .CountAsync(c => c.AffiliateProfileId == affiliateProfileId
                        && c.ClickedAt >= DateTime.UtcNow.AddDays(-7)),
                WeeklyConversions = 0, // TODO: Thêm logic tính conversion
                ConversionRate = 0 // TODO: Thêm logic tính tỷ lệ
            };
        }
        public async Task<AffiliateProductLink> GetAffiliateProductLinkByReferralCode(string referralCode)
        {
            // Tìm kiếm trong bảng AffiliateProductLinks theo referralCode
            var affiliateProductLink = await _context.AffiliateProductLinks
                .FirstOrDefaultAsync(l => l.ReferralCode == referralCode);

            // Nếu không tìm thấy referralCode hợp lệ, trả về null
            if (affiliateProductLink == null)
            {
                return null;
            }

            return affiliateProductLink; // Trả về thông tin liên quan đến link affiliate và sản phẩm
        }


        public async Task<List<TopProductDto>> GetTopProducts(Guid affiliateProfileId, int topCount)
        {
            var query = (
                from p in _context.Products
                join ct in _context.ClickTrackings on p.ProductId equals ct.ProductId into Clicks
                join od in _context.OrderDetails on p.ProductId equals od.ProductId into Orders
                from c in Clicks.DefaultIfEmpty()
                from o in Orders.DefaultIfEmpty()
                where c != null && c.AffiliateProfileId == affiliateProfileId
                   || (o != null && o.Order.AffiliateProfileId == affiliateProfileId)
                group new { c, o } by new { p.ProductId, p.Name, p.ImageUrls } into g
                select new TopProductDto
                {
                    ProductName = g.Key.Name,
                    ImageUrl = g.Key.ImageUrls != null ? string.Join(",", g.Key.ImageUrls) : null,

                    Revenue = g.Sum(x => x.o != null ? x.o.Quantity * (x.o.UnitPrice ?? 0) : 0),

                    Clicks = g.Count(x => x.c != null),
                    TotalOrders = g.Count(x => x.o != null),
                    ConversionRate = g.Count(x => x.c != null) == 0 ? 0 :
                                    (double)g.Count(x => x.o != null) / g.Count(x => x.c != null) * 100
                })
                .OrderByDescending(x => x.Revenue)
                .Take(topCount);

            return await query.ToListAsync();
        }







        // 🆕 Lấy số lần click theo AffiliateProfileId
        public async Task<int> GetClickCount(Guid affiliateProfileId)
        {
            return await _context.ClickTrackings
                .CountAsync(c => c.AffiliateProfileId == affiliateProfileId);
        }

        // 🆕 Lấy danh sách các lần click của một Affiliate
        public async Task<List<ClickTracking>> GetClickTrackingByAffiliate(Guid affiliateProfileId)
        {
            return await _context.ClickTrackings
                .Where(c => c.AffiliateProfileId == affiliateProfileId)
                .OrderByDescending(c => c.ClickedAt)
                .ToListAsync();
        }

        public Task<bool> RegisterAffiliate(RegisterAffiliateDto dto, int userId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateUserRole(int userId, int roleType)
        {
            throw new NotImplementedException();
        }

        public Task<string> GenerateAffiliateLink(Guid affiliateProfileId, Guid productId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> TrackClick(string referralCode, DateTime clickTime)
        {
            throw new NotImplementedException();
        }
    }
}
