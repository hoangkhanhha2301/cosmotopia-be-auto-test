namespace Cosmetics.DTO.Affiliate
{
    public class TransactionAffiliateExtendedDTO
    {
        public Guid TransactionAffiliatesId { get; set; }
        public Guid? AffiliateProfileId { get; set; }
        public decimal Amount { get; set; }
        public DateTime? TransactionDate { get; set; } // Thay đổi thành DateTime?
        public string Status { get; set; }
        public string AffiliateName { get; set; }
        public string Email { get; set; }
        public string BankName { get; set; }
        public string BankAccountNumber { get; set; }
    }
}
