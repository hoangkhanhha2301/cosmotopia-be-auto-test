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

        public async Task<bool> RegisterAffiliate(RegisterAffiliateDto dto, int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.RoleType != 3)
                throw new Exception("User không hợp lệ hoặc đã là Affiliate!");

            var existingProfile = await _context.AffiliateProfiles
                .FirstOrDefaultAsync(a => a.UserId == userId);
            if (existingProfile != null)
                throw new Exception("Bạn đã là Affiliate!");

            var affiliateProfile = new AffiliateProfile
            {
                AffiliateProfileId = Guid.NewGuid(),
                UserId = userId,
                BankName = dto.BankName,
                BankAccountNumber = dto.BankAccountNumber,
                BankBranch = dto.BankBranch,
                ReferralCode = Guid.NewGuid().ToString().Substring(0, 8),
                CreatedAt = DateTime.UtcNow
            };

            _context.AffiliateProfiles.Add(affiliateProfile);
            user.RoleType = 2;

            await _context.SaveChangesAsync();
            return true;
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




        public async Task<string> GenerateAffiliateLink(Guid affiliateProfileId, Guid productId)
        {
            var existingLink = await _context.AffiliateProductLinks
                .FirstOrDefaultAsync(l => l.AffiliateProfileId == affiliateProfileId && l.ProductId == productId);

            if (existingLink != null)
            {
                return $"http://localhost:3000/product/{productId}?ref={existingLink.ReferralCode}";
            }

            string referralCode = Guid.NewGuid().ToString().Substring(0, 8);

            var newLink = new AffiliateProductLink
            {
                AffiliateProfileId = affiliateProfileId,
                ProductId = productId,
                ReferralCode = referralCode,
                CreatedAt = DateTime.UtcNow
            };

            _context.AffiliateProductLinks.Add(newLink);
            await _context.SaveChangesAsync();

            return $"http://localhost:3000/product/{productId}?ref={referralCode}";
        }

        // 🆕 Track Click (Thêm lượt click vào bảng ClickTracking)
        public async Task<bool> TrackClick(string referralCode, DateTime clickTime)
        {
            var affiliateLink = await _context.AffiliateProductLinks
                .FirstOrDefaultAsync(l => l.ReferralCode == referralCode);

            if (affiliateLink == null)
            {
                return false;
            }

            var click = new ClickTracking
            {
                AffiliateProfileId = affiliateLink.AffiliateProfileId,
                ProductId = affiliateLink.ProductId,
                ReferralCode = referralCode,
                ClickedAt = clickTime
            };

            _context.ClickTrackings.Add(click);
            await _context.SaveChangesAsync();

            return true;
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

        public async Task UpdateUserRole(int userId, int roleType)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            user.RoleType = roleType; // Giả sử User có cột RoleType để lưu vai trò
            await _context.SaveChangesAsync();
        }

    }
}
