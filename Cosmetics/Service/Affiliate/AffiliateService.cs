using Cosmetics.DTO.Affiliate;
using Cosmetics.Models;
using Microsoft.EntityFrameworkCore;
using System;
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

        public async Task RegisterAffiliate(AffiliateProfile profile)
        {
            _context.AffiliateProfiles.Add(profile);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserRole(int userId, int roleType)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.RoleType = roleType;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<AffiliateProfile> GetAffiliateProfile(int userId)
        {
            return await _context.AffiliateProfiles
                .FirstOrDefaultAsync(ap => ap.UserId == userId);
        }

        public async Task<AffiliateIncomeDto> GetWeeklyIncome(Guid affiliateProfileId)
        {
            // Logic tính thu nhập tuần (có thể để tạm như sau)
            var profile = await _context.AffiliateProfiles.FindAsync(affiliateProfileId);
            return new AffiliateIncomeDto
            {
                TotalEarnings = profile?.TotalEarnings ?? 0,
                PendingAmount = profile?.PendingAmount ?? 0,
                WithdrawnAmount = profile?.WithdrawnAmount ?? 0,
                WeeklyClicks = 0, // Thêm logic tính clicks
                WeeklyConversions = 0, // Thêm logic tính conversions
                ConversionRate = 0 // Thêm logic tính tỷ lệ
            };
        }

        public async Task<List<TopProductDto>> GetTopProducts(Guid affiliateProfileId, int topCount)
        {
            // Logic lấy top sản phẩm (tạm trả về rỗng)
            return new List<TopProductDto>();
        }

        public async Task<string> GenerateAffiliateLink(Guid affiliateProfileId, Guid productId)
        {
            // Logic tạo link (tạm trả về mẫu)
            return $"http://localhost:3000/product/{productId}?ref={affiliateProfileId}";
        }

        public async Task<bool> TrackClick(string referralCode, DateTime clickTime)
        {
            // Logic theo dõi click (tạm trả về true)
            return true;
        }
    }
}