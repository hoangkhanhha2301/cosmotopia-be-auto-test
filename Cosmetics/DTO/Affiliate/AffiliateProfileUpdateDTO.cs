namespace Cosmetics.DTO.Affiliate
{
    public class AffiliateProfileUpdateDTO
    {
        public Guid AffiliateProfileId { get; set; }
        public string BankAccount { get; set; }
        public string BankName { get; set; }
        public string PaymentMethod { get; set; }
    }
}
