using Cosmetics.DTO.Affiliate;
using Cosmetics.Enum;

namespace Cosmetics.Service.Affiliate.Interface
{
    public interface IAffiliateService
    {
        Task<AffiliateProfileDto> RegisterAffiliateAsync(int userId, AffiliateRegistrationRequestDto request);
        Task<AffiliateLinkDto> GenerateAffiliateLinkAsync(int userId, Guid productId);
        Task<AffiliateStatsDto> GetAffiliateStatsAsync(int userId, DateTime startDate, DateTime endDate);
        Task<TransactionAffiliateDTO> RequestWithdrawalAsync(int userId, WithdrawalRequestDto request);
        Task<TransactionAffiliateDTO> UpdateWithdrawalStatusAsync(Guid transactionId, WithdrawalStatus status);

        Task TrackAffiliateClickAsync(string referralCode, int? userId);

        Task<AffiliateProfileResponseDto> GetAffiliateProfileAsync(int userId);

        Task<List<TransactionAffiliateDTO>> GetWithdrawalsByAffiliateAsync(int userId);

        Task<List<TransactionAffiliateExtendedDTO>> GetAllWithdrawalsAsync();
        Task<List<AffiliateLinkExtendedDto>> GetAllLinksAsync(int userId);

        Task<List<AffiliateEarningsDto>> GetAllEarningsAsync(int userId);

        Task<AffiliateSummaryDto> GetAffiliateSummaryAsync(int userId);

    }
}
