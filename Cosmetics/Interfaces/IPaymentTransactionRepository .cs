using Cosmetics.Models;
using Cosmetics.Repositories;

namespace Cosmetics.Interfaces
{
    public interface IPaymentTransactionRepository : IGenericRepository<PaymentTransaction> 
    {
        Task<PaymentTransaction?> GetByTransactionIdAsync(string transactionId);
        Task<Order?> GetOrderById(Guid id);
    }
}
