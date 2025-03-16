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

        // Đăng ký Affiliate (sử dụng DTO thay vì model)
        Task<bool> RegisterAffiliate(RegisterAffiliateDto dto, int userId);

        // Cập nhật Role của User
        Task UpdateUserRole(int userId, int roleType);

        // Lấy thông tin Affiliate Profile dựa trên UserId
        Task<AffiliateProfile> GetAffiliateProfile(int userId);

        // Lấy thu nhập hàng tuần của Affiliate (sử dụng Guid)
        Task<AffiliateIncomeDto> GetWeeklyIncome(Guid affiliateProfileId);

        // Lấy danh sách sản phẩm hàng đầu của Affiliate (sử dụng Guid)
        Task<List<TopProductDto>> GetTopProducts(Guid affiliateProfileId, int topCount);

        // Tạo link tiếp thị liên kết cho sản phẩm (sử dụng Guid)
        Task<string> GenerateAffiliateLink(Guid affiliateProfileId, Guid productId);

        // Ghi nhận lượt click vào link tiếp thị
        Task<bool> TrackClick(string referralCode, DateTime clickTime);

        // Đếm số lượt click theo AffiliateProfileId
        Task<int> GetClickCount(Guid affiliateProfileId);

        // Lấy danh sách chi tiết lượt click theo AffiliateProfileId
        Task<List<ClickTracking>> GetClickTrackingByAffiliate(Guid affiliateProfileId);
    }
}
