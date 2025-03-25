namespace Cosmetics.DTO.Affiliate
{
    public class AffiliateProfileDto
    {
        public Guid AffiliateProfileId { get; set; }
        public int UserId { get; set; }
        public string BankName { get; set; }
        public string BankAccountNumber { get; set; }
        public string? BankBranch { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal Balance { get; set; }
        public decimal PendingAmount { get; set; }
        public decimal WithdrawnAmount { get; set; }
        public string ReferralCode { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
