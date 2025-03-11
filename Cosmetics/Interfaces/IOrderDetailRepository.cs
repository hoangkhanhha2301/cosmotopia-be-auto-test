using Cosmetics.Models;

namespace Cosmetics.Interfaces
{
    public interface IOrderDetailRepository : IGenericRepository<OrderDetail>
    {
        Task<IEnumerable<OrderDetail>> GetByOrderIdAsync(Guid orderId);
    }
}