namespace Cosmetics.DTO.Affiliate
{
    public class AffiliateProfileDTO
    {
        public Guid AffiliateProfileId { get; set; }
        public int UserId { get; set; }
        public string BankAccount { get; set; }
        public string BankName { get; set; }
        public string PaymentMethod { get; set; }
        public string ApplicationStatus { get; set; }
        public DateTime? CreateAt { get; set; }
        public decimal? TotalEarnings { get; set; }
        public bool? IsActive { get; set; }
    
}
}
