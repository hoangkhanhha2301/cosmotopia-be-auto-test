namespace Cosmetics.DTO.Affiliate
{
    public class WithdrawalResponseDto
    {
        public Guid TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } // "Pending", "Paid", "Failed"
        public DateTime TransactionDate { get; set; }
    }
}
