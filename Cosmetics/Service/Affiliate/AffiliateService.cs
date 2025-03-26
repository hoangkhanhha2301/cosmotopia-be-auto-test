using AutoMapper;
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
        private readonly IMapper _mapper;

        public AffiliateService(IAffiliateRepository affiliateRepository, ComedicShopDBContext context, IMapper mapper)
        {
            _affiliateRepository = affiliateRepository;
            _context = context;
            _mapper = mapper;
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

        public async Task<TransactionAffiliateDTO> RequestWithdrawalAsync(int userId, WithdrawalRequestDto request)
        {
            var affiliateProfile = await _affiliateRepository.GetAffiliateProfileByUserIdAsync(userId);
            if (affiliateProfile == null) throw new Exception("Affiliate profile not found.");

            if (request.Amount <= 0) throw new Exception("Withdrawal amount must be greater than 0.");
            if (request.Amount > affiliateProfile.Ballance) throw new Exception("Insufficient balance.");

            var transaction = new TransactionAffiliate
            {
                AffiliateProfileId = affiliateProfile.AffiliateProfileId,
                Amount = request.Amount,
                TransactionDate = DateTime.UtcNow,
                Status = TransactionStatus.Pending.ToString()
            };

            var createdTransaction = await _affiliateRepository.CreateTransactionAsync(transaction);

            affiliateProfile.Ballance -= request.Amount;
            affiliateProfile.PendingAmount += request.Amount;
            await _affiliateRepository.UpdateAffiliateProfileAsync(affiliateProfile);

            return _mapper.Map<TransactionAffiliateDTO>(createdTransaction);
        }
        public async Task<TransactionAffiliateDTO> UpdateWithdrawalStatusAsync(Guid transactionId, WithdrawalStatus status)
        {
            var transaction = await _context.TransactionAffiliates
                .FirstOrDefaultAsync(t => t.TransactionAffiliatesId == transactionId);
            if (transaction == null) throw new Exception("Transaction not found.");

            var affiliateProfile = await _context.AffiliateProfiles
                .FirstOrDefaultAsync(ap => ap.AffiliateProfileId == transaction.AffiliateProfileId);
            if (affiliateProfile == null) throw new Exception("Affiliate profile not found.");

            // Chuyển enum thành string để so sánh và lưu vào DB
            string newStatus = status.Status.ToString();

            // Nếu trạng thái không thay đổi, không cần xử lý
            if (transaction.Status == newStatus) return _mapper.Map<TransactionAffiliateDTO>(transaction);

            // Xử lý theo trạng thái mới
            if (status.Status.Equals(TransactionStatus.Paid)) // Sử dụng Equals thay vì ==
            {
                // Trừ PendingAmount và cộng vào WithdrawnAmount
                affiliateProfile.PendingAmount -= transaction.Amount;
                affiliateProfile.WithdrawnAmount += transaction.Amount;
            }
            else if (status.Status.Equals(TransactionStatus.Failed)) // Sử dụng Equals thay vì ==
            {
                // Cộng lại Balance và trừ PendingAmount
                affiliateProfile.Ballance += transaction.Amount;
                affiliateProfile.PendingAmount -= transaction.Amount;
            }

            // Cập nhật trạng thái giao dịch
            transaction.Status = newStatus;
            await _context.SaveChangesAsync();

            return _mapper.Map<TransactionAffiliateDTO>(transaction);
        }
        public async Task<AffiliateProfileResponseDto> GetAffiliateProfileAsync(int userId)
        {
            // Lấy thông tin từ AffiliateProfiles
            var affiliateProfile = await _affiliateRepository.GetAffiliateProfileByUserIdAsync(userId);
            if (affiliateProfile == null) throw new Exception("Affiliate profile not found.");

            // Lấy thông tin từ Users
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) throw new Exception("User not found.");

            // Kết hợp dữ liệu vào DTO
            var response = new AffiliateProfileResponseDto
            {
                AffiliateProfileId = affiliateProfile.AffiliateProfileId,
                UserId = affiliateProfile.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                RoleType = user.RoleType,
                BankName = affiliateProfile.BankName,
                BankAccountNumber = affiliateProfile.BankAccountNumber,
                BankBranch = affiliateProfile.BankBranch,
                TotalEarnings = affiliateProfile.TotalEarnings ?? 0m,
                Balance = affiliateProfile.Ballance ?? 0m,
                PendingAmount = affiliateProfile.PendingAmount ?? 0m,
                WithdrawnAmount = affiliateProfile.WithdrawnAmount ?? 0m,
            };

            return response;
        }
        public async Task<List<TransactionAffiliateDTO>> GetWithdrawalsByAffiliateAsync(int userId)
        {
            var affiliateProfile = await _affiliateRepository.GetAffiliateProfileByUserIdAsync(userId);
            if (affiliateProfile == null) throw new Exception("Affiliate profile not found.");

            var withdrawals = await _context.TransactionAffiliates
                .Where(t => t.AffiliateProfileId == affiliateProfile.AffiliateProfileId)
                .ToListAsync();

            return _mapper.Map<List<TransactionAffiliateDTO>>(withdrawals);
        }



        public async Task<List<TransactionAffiliateExtendedDTO>> GetAllWithdrawalsAsync()
        {
            var withdrawals = await _context.TransactionAffiliates
                .Include(t => t.AffiliateProfile)
                .ThenInclude(ap => ap.User)
                .Select(t => new TransactionAffiliateExtendedDTO
                {
                    TransactionAffiliatesId = t.TransactionAffiliatesId,
                    AffiliateProfileId = t.AffiliateProfileId ?? Guid.Empty, // Xử lý null
                    Amount = t.Amount,
                    TransactionDate = t.TransactionDate ?? DateTime.UtcNow,
                    Status = t.Status,
                    AffiliateName = t.AffiliateProfile.User.FirstName + " " + t.AffiliateProfile.User.LastName,
                    Email = t.AffiliateProfile.User.Email,
                    BankName = t.AffiliateProfile.BankName,
                    BankAccountNumber = t.AffiliateProfile.BankAccountNumber
                })
                .ToListAsync();

            return withdrawals;
        }
    }
}