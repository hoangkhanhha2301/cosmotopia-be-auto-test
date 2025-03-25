using Cosmetics.Models;

namespace Cosmetics.Repositories.Interface
{
    public interface IAffiliateRepository
    {
        Task<AffiliateProfile> GetAffiliateProfileByUserIdAsync(int userId);
        Task<AffiliateProfile> CreateAffiliateProfileAsync(AffiliateProfile profile);
        Task<AffiliateProductLink> CreateAffiliateLinkAsync(AffiliateProductLink link);
        Task<ClickTracking> GetClickTrackingByUserIdAndLinkIdAsync(int userId, int linkId);
        Task<ClickTracking> CreateClickTrackingAsync(ClickTracking click);
        Task UpdateClickTrackingAsync(ClickTracking click);
        Task<List<ClickTracking>> GetClickTrackingsByAffiliateIdAsync(Guid affiliateProfileId, DateTime startDate, DateTime endDate);
        Task<TransactionAffiliate> CreateWithdrawalAsync(TransactionAffiliate withdrawal);
        Task UpdateWithdrawalAsync(TransactionAffiliate withdrawal);

        Task<AffiliateProductLink> GetAffiliateLinkByReferralCodeAsync(string referralCode);

        Task<ClickTracking> GetClickTrackingByReferralCodeAsync(string referralCode, int? userId);

        Task AddClickTrackingAsync(ClickTracking clickTracking);

        Task<TransactionAffiliate> CreateTransactionAsync(TransactionAffiliate transaction); // Đã có sẵn

        Task UpdateAffiliateProfileAsync(AffiliateProfile profile); // Đã có sẵn
    }
}
