using System;
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
            // Kiểm tra xem Affiliate đã tạo link cho sản phẩm này chưa
            var existingLink = await _context.AffiliateProductLinks
                .FirstOrDefaultAsync(al => al.AffiliateProfileId == profile.AffiliateProfileId && al.ProductId == productId);
            if (existingLink != null)
            {
                throw new Exception("You have already generated a link for this product.");
            }

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
        .Include(t => t.TransactionDetail)
        .FirstOrDefaultAsync(t => t.TransactionAffiliatesId == transactionId);
            if (transaction == null) throw new Exception("Transaction not found.");

            var affiliateProfile = await _context.AffiliateProfiles
                .FirstOrDefaultAsync(ap => ap.AffiliateProfileId == transaction.AffiliateProfileId);
            if (affiliateProfile == null) throw new Exception("Affiliate profile not found.");

            var validStatuses = new[] { "Pending", "Paid", "Failed" };
            if (!validStatuses.Contains(status.Status, StringComparer.OrdinalIgnoreCase))
            {
                throw new Exception("Invalid status value. Use 'Pending', 'Paid', or 'Failed'.");
            }

            // So sánh status và image để tránh cập nhật không cần thiết
            if (transaction.Status == status.Status &&
                (transaction.TransactionDetail == null || transaction.TransactionDetail.Image == status.Image))
            {
                return _mapper.Map<TransactionAffiliateDTO>(transaction);
            }

            // Cập nhật số dư dựa trên status
            if (status.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase))
            {
                affiliateProfile.PendingAmount -= transaction.Amount;
                affiliateProfile.WithdrawnAmount += transaction.Amount;
            }
            else if (status.Status.Equals("Failed", StringComparison.OrdinalIgnoreCase))
            {
                affiliateProfile.Ballance += transaction.Amount;
                affiliateProfile.PendingAmount -= transaction.Amount;
            }

            // Cập nhật status
            transaction.Status = status.Status;

            // Cập nhật image
            if (!string.IsNullOrEmpty(status.Image))
            {
                if (transaction.TransactionDetail == null)
                {
                    transaction.TransactionDetail = new TransactionDetail
                    {
                        TransactionDetailId = Guid.NewGuid(),
                        TransactionAffiliatesId = transaction.TransactionAffiliatesId,
                        Image = status.Image
                    };
                    _context.TransactionDetails.Add(transaction.TransactionDetail);
                }
                else
                {
                    transaction.TransactionDetail.Image = status.Image;
                }
            }

            await _context.SaveChangesAsync();
            await _affiliateRepository.UpdateAffiliateProfileAsync(affiliateProfile);

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
                .Include(t => t.TransactionDetail)
                .ToListAsync(); // Lấy dữ liệu thô trước

            var result = withdrawals.Select(t => new TransactionAffiliateExtendedDTO
            {
                TransactionAffiliatesId = t.TransactionAffiliatesId,
                AffiliateProfileId = t.AffiliateProfileId ?? Guid.Empty,
                Amount = t.Amount,
                TransactionDate = t.TransactionDate ?? DateTime.UtcNow, // Loại bỏ ?? DateTime.UtcNow
                Status = t.Status,
                Image = t.TransactionDetail != null ? t.TransactionDetail.Image : null, // Đã sửa kiểu Image thành string
                FirstName = t.AffiliateProfile != null && t.AffiliateProfile.User != null ? t.AffiliateProfile.User.FirstName : "N/A",
                LastName = t.AffiliateProfile != null && t.AffiliateProfile.User != null ? t.AffiliateProfile.User.LastName : "N/A",
                AffiliateName = t.AffiliateProfile != null && t.AffiliateProfile.User != null ? $"{t.AffiliateProfile.User.FirstName} {t.AffiliateProfile.User.LastName}" : "N/A",
                Email = t.AffiliateProfile != null && t.AffiliateProfile.User != null ? t.AffiliateProfile.User.Email : "N/A",
                BankName = t.AffiliateProfile != null ? t.AffiliateProfile.BankName : "N/A",
                BankAccountNumber = t.AffiliateProfile != null ? t.AffiliateProfile.BankAccountNumber : "N/A"
            }).ToList();

            return result;
        }

        public async Task<List<AffiliateLinkExtendedDto>> GetAllLinksAsync(int userId)
        {
            var affiliateProfile = await _affiliateRepository.GetAffiliateProfileByUserIdAsync(userId);
            if (affiliateProfile == null)
            {
                throw new Exception("Affiliate profile not found.");
            }

            var links = await _context.AffiliateProductLinks
                .Include(al => al.Product)
                .Where(al => al.AffiliateProfileId == affiliateProfile.AffiliateProfileId)
                .Select(al => new AffiliateLinkExtendedDto
                {
                    LinkId = al.LinkId,
                    AffiliateProfileId = al.AffiliateProfileId,
                    ProductId = al.ProductId,
                    ProductName = al.Product.Name,
                    Price = al.Product.Price,
                    CommissionRate = al.Product.CommissionRate ?? 0m,
                    Image = string.Join(",", al.Product.ImageUrls),
                    ReferralCode = al.ReferralCode,
                    CreatedAt = al.CreatedAt ?? DateTime.UtcNow,
                })
                .ToListAsync();

            return links;
        }
    }
}