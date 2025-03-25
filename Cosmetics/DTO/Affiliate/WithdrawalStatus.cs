using System.Transactions;

namespace Cosmetics.DTO.Affiliate
{
    public class WithdrawalStatus
    {
        public TransactionStatus Status { get; set; } // Sử dụng enum
    }
}
