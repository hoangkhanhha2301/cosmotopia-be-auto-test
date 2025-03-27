namespace Cosmetics.DTO.Affiliate
{
    public class TransactionAffiliateExtendedDTO
    {
        public Guid TransactionAffiliatesId { get; set; }
        public Guid AffiliateProfileId { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Status { get; set; }
        public string Image { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string AffiliateName { get; set; }
        public string Email { get; set; }
        public string BankName { get; set; }
        public string BankAccountNumber { get; set; }
    }
}
