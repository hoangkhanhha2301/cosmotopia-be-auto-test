namespace Cosmetics.DTO.Affiliate
{
    public class AffiliateProfileResponseDto
    {
        public Guid AffiliateProfileId { get; set; }
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int RoleType { get; set; }
        public string BankName { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankBranch { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal Balance { get; set; }
        public decimal PendingAmount { get; set; }
        public decimal WithdrawnAmount { get; set; }
    }
}
