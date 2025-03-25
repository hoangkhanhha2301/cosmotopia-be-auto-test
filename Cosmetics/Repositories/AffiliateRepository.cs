using Cosmetics.Models;
using Cosmetics.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace Cosmetics.Repositories
{
    public class AffiliateRepository : IAffiliateRepository
    {
        private readonly ComedicShopDBContext _context;

        public AffiliateRepository(ComedicShopDBContext context)
        {
            _context = context;
        }

        public async Task<AffiliateProfile> GetAffiliateProfileByUserIdAsync(int userId)
        {
            return await _context.AffiliateProfiles.FirstOrDefaultAsync(ap => ap.UserId == userId);
        }

        public async Task<AffiliateProfile> CreateAffiliateProfileAsync(AffiliateProfile profile)
        {
            _context.AffiliateProfiles.Add(profile);
            await _context.SaveChangesAsync();
            return profile;
        }

        public async Task<AffiliateProductLink> CreateAffiliateLinkAsync(AffiliateProductLink link)
        {
            _context.AffiliateProductLinks.Add(link);
            await _context.SaveChangesAsync();
            return link;
        }

        public async Task<ClickTracking> GetClickTrackingByUserIdAndLinkIdAsync(int userId, int linkId)
        {
            return await _context.ClickTrackings
                .FirstOrDefaultAsync(ct => ct.UserId == userId && ct.LinkId == linkId);
        }

        public async Task<ClickTracking> CreateClickTrackingAsync(ClickTracking click)
        {
            _context.ClickTrackings.Add(click);
            await _context.SaveChangesAsync();
            return click;
        }

        public async Task UpdateClickTrackingAsync(ClickTracking click)
        {
            _context.ClickTrackings.Update(click);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ClickTracking>> GetClickTrackingsByAffiliateIdAsync(Guid affiliateProfileId, DateTime startDate, DateTime endDate)
        {
            return await _context.ClickTrackings
                .Join(_context.AffiliateProductLinks,
                      ct => ct.LinkId,
                      apl => apl.LinkId,
                      (ct, apl) => new { ClickTracking = ct, AffiliateProductLink = apl })
                .Where(joined => joined.AffiliateProductLink.AffiliateProfileId == affiliateProfileId
                              && joined.ClickTracking.ClickedAt >= startDate
                              && joined.ClickTracking.ClickedAt <= endDate)
                .Select(joined => joined.ClickTracking)
                .ToListAsync();
        }

        public async Task<TransactionAffiliate> CreateWithdrawalAsync(TransactionAffiliate withdrawal)
        {
            _context.TransactionAffiliates.Add(withdrawal);
            await _context.SaveChangesAsync();
            return withdrawal;
        }

        public async Task UpdateWithdrawalAsync(TransactionAffiliate withdrawal)
        {
            _context.TransactionAffiliates.Update(withdrawal);
            await _context.SaveChangesAsync();
        }

        public async Task<ClickTracking> GetClickTrackingByReferralCodeAsync(string referralCode, int? userId)
        {
            return await _context.ClickTrackings
                .FirstOrDefaultAsync(ct => ct.ReferralCode == referralCode && ct.UserId == userId);
        }

        public async Task<AffiliateProductLink> GetAffiliateLinkByReferralCodeAsync(string referralCode)
        {
            return await _context.AffiliateProductLinks
                .FirstOrDefaultAsync(apl => apl.ReferralCode == referralCode);
        }

        public async Task AddClickTrackingAsync(ClickTracking clickTracking)
        {
            await _context.ClickTrackings.AddAsync(clickTracking);
            await _context.SaveChangesAsync();
        }

        public async Task<TransactionAffiliate> CreateTransactionAsync(TransactionAffiliate transaction)
        {
            transaction.TransactionAffiliatesId = Guid.NewGuid(); // Tạo ID mới
            await _context.TransactionAffiliates.AddAsync(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }
        public async Task UpdateAffiliateProfileAsync(AffiliateProfile profile)
        {
            var existingProfile = await _context.AffiliateProfiles
                .FirstOrDefaultAsync(ap => ap.AffiliateProfileId == profile.AffiliateProfileId);

            if (existingProfile == null) throw new Exception("Affiliate profile not found.");

            // Cập nhật các trường cần thiết
            existingProfile.BankName = profile.BankName;
            existingProfile.BankAccountNumber = profile.BankAccountNumber;
            existingProfile.BankBranch = profile.BankBranch;
            existingProfile.TotalEarnings = profile.TotalEarnings;
            existingProfile.Ballance = profile.Ballance;
            existingProfile.PendingAmount = profile.PendingAmount;
            existingProfile.WithdrawnAmount = profile.WithdrawnAmount;
            existingProfile.ReferralCode = profile.ReferralCode;

            _context.AffiliateProfiles.Update(existingProfile);
            await _context.SaveChangesAsync();
        }
    }
}