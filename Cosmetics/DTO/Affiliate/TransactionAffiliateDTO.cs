namespace Cosmetics.DTO.Affiliate
{
    public class TransactionAffiliateDTO
    {
        public Guid TransactionAffiliatesId { get; set; }
        public Guid AffiliateProfileId { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Status { get; set; }
    }
}
