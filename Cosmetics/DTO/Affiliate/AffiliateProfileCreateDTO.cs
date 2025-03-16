namespace Cosmetics.DTO.Affiliate
{
    public class AffiliateProfileCreateDTO
    {
        public int UserId { get; set; } // Required: Links to existing user
        public string BankAccount { get; set; } // Optional
        public string BankName { get; set; } // Optional
        public string PaymentMethod { get; set; } // Optional (e.g., "Bank Transfer", "MoMo")
    }
}
