using Cosmetics.DTO.Affiliate;
using Cosmetics.Enum;
using Cosmetics.Models;
using Cosmetics.Repositories.Interface;
using Cosmetics.Service.Affiliate.Interface;
using Microsoft.EntityFrameworkCore;

namespace Cosmetics.Service.Affiliate
{
    public class AffiliateService : IAffiliateService
    {
        private readonly IAffiliateRepository _affiliateRepository;
        private readonly ComedicShopDBContext _context;

        public AffiliateService(IAffiliateRepository affiliateRepository, ComedicShopDBContext context)
        {
            _affiliateRepository = affiliateRepository;
            _context = context;
        }

        public async Task<AffiliateProfileDto> RegisterAffiliateAsync(int userId, AffiliateRegistrationRequestDto request)
        {
            var existingProfile = await _affiliateRepository.GetAffiliateProfileByUserIdAsync(userId);
            if (existingProfile != null)
                throw new Exception("Bạn đã là Affiliate rồi!");

            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.RoleType != 3) // Customer
                throw new Exception("Bạn phải là Customer để đăng ký Affiliate!");

            var profile = new AffiliateProfile
            {
                UserId = userId,
                BankName = request.BankName, // Có thể là số hoặc chữ (ví dụ: "Vietcombank" hoặc "123")
                BankAccountNumber = request.BankAccountNumber, // Có thể là số hoặc chữ (ví dụ: "123456789" hoặc "ABC123")
                BankBranch = request.BankBranch, // Có thể là số hoặc chữ, không bắt buộc (ví dụ: "Branch 01" hoặc "1")
                ReferralCode = Guid.NewGuid().ToString("N").Substring(0, 8),
                CreatedAt = DateTime.UtcNow
            };

            var createdProfile = await _affiliateRepository.CreateAffiliateProfileAsync(profile);
            user.RoleType = 2; // Chuyển thành Affiliate
            await _context.SaveChangesAsync();

            return new AffiliateProfileDto
            {
                AffiliateProfileId = createdProfile.AffiliateProfileId,
                UserId = createdProfile.UserId,
                BankName = createdProfile.BankName,
                BankAccountNumber = createdProfile.BankAccountNumber,
                BankBranch = createdProfile.BankBranch,
                ReferralCode = createdProfile.ReferralCode,
                TotalEarnings = createdProfile.TotalEarnings ?? 0m,
                Balance = createdProfile.Ballance ?? 0m,
                PendingAmount = createdProfile.PendingAmount ?? 0m,
                WithdrawnAmount = createdProfile.WithdrawnAmount ?? 0m,
                CreatedAt = createdProfile.CreatedAt ?? DateTime.UtcNow
            };
        }

        public async Task<AffiliateLinkDto> GenerateAffiliateLinkAsync(int userId, Guid productId)
        {
            var profile = await _affiliateRepository.GetAffiliateProfileByUserIdAsync(userId);
            if (profile == null) throw new Exception("Affiliate profile not found.");

            var product = await _context.Products.FindAsync(productId);
            if (product == null) throw new Exception("Product not found.");

            var referralCode = $"{profile.ReferralCode}-{Guid.NewGuid().ToString("N").Substring(0, 6)}";
            var affiliateLink = new AffiliateProductLink
            {
                AffiliateProfileId = profile.AffiliateProfileId,
                ProductId = productId,
                ReferralCode = referralCode,
                CreatedAt = DateTime.UtcNow
            };

            var createdLink = await _affiliateRepository.CreateAffiliateLinkAsync(affiliateLink);

            // Use the CreatedAt value from the createdLink, as that's the time the link was created
            return new AffiliateLinkDto
            {
                LinkId = createdLink.LinkId,
                AffiliateProfileId = createdLink.AffiliateProfileId,
                ProductId = createdLink.ProductId,
                ReferralCode = createdLink.ReferralCode,
                CreatedAt = DateTime.UtcNow,  
                AffiliateProductUrl = $"https://yourdomain.com/product/{productId}?ref={referralCode}"
            };
        }


        public async Task TrackAffiliateClickAsync(string referralCode, int? userId)
        {
            var affiliateLink = await _affiliateRepository.GetAffiliateLinkByReferralCodeAsync(referralCode);
            if (affiliateLink == null) throw new Exception("Invalid referral code.");

            var existingClick = await _affiliateRepository.GetClickTrackingByReferralCodeAsync(referralCode, userId);
            if (existingClick != null)
            {
                existingClick.Count++;
                existingClick.ClickedAt = DateTime.UtcNow;
                await _affiliateRepository.UpdateClickTrackingAsync(existingClick);
            }
            else
            {
                var clickTracking = new ClickTracking
                {
                    LinkId = affiliateLink.LinkId,
                    UserId = userId,
                    Count = 1,
                    ReferralCode = referralCode,
                    ClickedAt = DateTime.UtcNow
                };
                await _affiliateRepository.AddClickTrackingAsync(clickTracking);
            }
        }

        public async Task<AffiliateStatsDto> GetAffiliateStatsAsync(int userId, DateTime startDate, DateTime endDate)
        {
            var profile = await _affiliateRepository.GetAffiliateProfileByUserIdAsync(userId);
            if (profile == null)
                throw new Exception("Bạn chưa là Affiliate!");

            var clicks = await _affiliateRepository.GetClickTrackingsByAffiliateIdAsync(profile.AffiliateProfileId, startDate, endDate);
            var conversions = await _context.AffiliateCommissions
                .Where(ac => ac.AffiliateProfileId == profile.AffiliateProfileId && ac.EarnedAt >= startDate && ac.EarnedAt <= endDate)
                .ToListAsync();

            return new AffiliateStatsDto
            {
                WeeklyEarnings = conversions.Sum(c => c.CommissionAmount),
                Count = clicks.Sum(c => c.Count ?? 0),
                ConversionCount = conversions.Count
            };
        }

        public async Task<WithdrawalResponseDto> RequestWithdrawalAsync(int userId, WithdrawalRequestDto request)
        {
            var profile = await _affiliateRepository.GetAffiliateProfileByUserIdAsync(userId);
            if (profile == null)
                throw new Exception("Bạn chưa là Affiliate!");
            if (profile.Ballance < request.Amount)
                throw new Exception("Số dư không đủ để rút!");

            var withdrawal = new TransactionAffiliate
            {
                AffiliateProfileId = profile.AffiliateProfileId,
                Amount = request.Amount,
                TransactionDate = DateTime.UtcNow,
                Status = "Pending"
            };

            profile.PendingAmount += request.Amount;
            profile.Ballance -= request.Amount;
            await _context.SaveChangesAsync();

            var createdWithdrawal = await _affiliateRepository.CreateWithdrawalAsync(withdrawal);
            return new WithdrawalResponseDto
            {
                TransactionId = createdWithdrawal.TransactionAffiliatesId,
                Amount = createdWithdrawal.Amount,
                Status = createdWithdrawal.Status,
                TransactionDate = createdWithdrawal.TransactionDate ?? DateTime.UtcNow
            };
        }

        public async Task<WithdrawalResponseDto> UpdateWithdrawalStatusAsync(Guid transactionId, WithdrawalStatus status)
        {
            var withdrawal = await _context.TransactionAffiliates.FindAsync(transactionId);
            if (withdrawal == null)
                throw new Exception("Giao dịch không tồn tại!");

            var profile = await _context.AffiliateProfiles.FindAsync(withdrawal.AffiliateProfileId);
            withdrawal.Status = status.ToString();

            if (status == WithdrawalStatus.Paid)
            {
                profile.WithdrawnAmount += withdrawal.Amount;
                profile.PendingAmount -= withdrawal.Amount;
            }
            else if (status == WithdrawalStatus.Failed)
            {
                profile.Ballance += withdrawal.Amount;
                profile.PendingAmount -= withdrawal.Amount;
            }

            await _affiliateRepository.UpdateWithdrawalAsync(withdrawal);
            await _context.SaveChangesAsync();

            return new WithdrawalResponseDto
            {
                TransactionId = withdrawal.TransactionAffiliatesId,
                Amount = withdrawal.Amount,
                Status = withdrawal.Status,
                TransactionDate = withdrawal.TransactionDate ?? DateTime.UtcNow
            };
        }

    }
}
