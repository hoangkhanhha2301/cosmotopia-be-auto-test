namespace Cosmetics.DTO.Affiliate
{
    public class AffiliateRegistrationRequestDto
    {
        public string BankName { get; set; }
        public string BankAccountNumber { get; set; }
        public string? BankBranch { get; set; } // Không bắt buộc
    }
}
