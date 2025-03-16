using Cosmetics.DTO.Affiliate;
using Cosmetics.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cosmetics.Service.Affiliate
{
    public interface IAffiliateService
    {
        Task<User> GetUserById(int userId);
        Task RegisterAffiliate(AffiliateProfile profile);
        Task UpdateUserRole(int userId, int roleType);
        Task<AffiliateProfile> GetAffiliateProfile(int userId); // Trả về profile dựa trên UserId
        Task<AffiliateIncomeDto> GetWeeklyIncome(Guid affiliateProfileId); // Dùng Guid
        Task<List<TopProductDto>> GetTopProducts(Guid affiliateProfileId, int topCount); // Dùng Guid
        Task<string> GenerateAffiliateLink(Guid affiliateProfileId, Guid productId); // Dùng Guid
        Task<bool> TrackClick(string referralCode, DateTime clickTime);
    }
}