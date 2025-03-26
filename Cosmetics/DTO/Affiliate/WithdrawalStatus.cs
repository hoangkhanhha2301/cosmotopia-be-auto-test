using System.Transactions;

namespace Cosmetics.DTO.Affiliate
{
    public class WithdrawalStatus
    {
        public string Status { get; set; } // "Pending", "Paid", hoặc "Failed"
    }
}
