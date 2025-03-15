using Cosmetics.Models;

namespace Cosmetics.Interfaces
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(int customerId);
        Task<Order?> GetByIdAsync(Guid id);
    }

}