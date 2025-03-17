using Cosmetics.Interfaces;
using Cosmetics.Models;
using Microsoft.EntityFrameworkCore;

namespace Cosmetics.Repositories
{
    public class PaymentTransactionRepository : GenericRepository<PaymentTransaction>, IPaymentTransactionRepository
    {
        public PaymentTransactionRepository(ComedicShopDBContext context) : base(context) { }


        public async Task<PaymentTransaction?> GetByTransactionIdAsync(string transactionId)
        {
            return await _context.PaymentTransactions
                .FirstOrDefaultAsync(p => p.TransactionId == transactionId)!;
        }
    }
}
