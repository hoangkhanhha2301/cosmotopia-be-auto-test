namespace Cosmetics.DTO.Affiliate
{
    public class AffiliateEarningsDto
    {
        public Guid ProductId { get; set; }
        public string ProductImageUrl { get; set; }
        public string ReferralCode { get; set; }
        public decimal TotalEarnings { get; set; }
    }
}
