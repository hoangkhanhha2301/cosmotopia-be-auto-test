namespace Cosmetics.DTO.Affiliate
{
    public class AffiliateLinkExtendedDto
    {
        public int LinkId { get; set; }
        public Guid AffiliateProfileId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public decimal CommissionRate { get; set; }
        public string[] Image { get; set; }
        public string ReferralCode { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
