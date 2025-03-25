namespace Cosmetics.DTO.Affiliate
{
    public class AffiliateLinkDto
    {
        public int LinkId { get; set; }
        public Guid AffiliateProfileId { get; set; }
        public Guid ProductId { get; set; }
        public string ReferralCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public string AffiliateProductUrl { get; set; }
    }
}
