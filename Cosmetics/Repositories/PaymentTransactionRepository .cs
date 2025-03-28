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
        public async Task<Order?> GetOrderById(Guid id)
        {
            return await _context.Orders
                // Nếu cần lấy luôn thông tin chi tiết đơn hàng

                .FirstOrDefaultAsync(o => o.OrderId == id);
        }
    }
}
