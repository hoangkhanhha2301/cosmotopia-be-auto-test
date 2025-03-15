using Cosmetics.Interfaces;
using Cosmetics.Models;
using Microsoft.EntityFrameworkCore;

namespace Cosmetics.Repositories
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        public OrderRepository(ComedicShopDBContext context) : base(context) { }

        public async Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(int customerId)
        {
            return await _dbSet
                .Where(o => o.CustomerId == customerId)
                .Include(o => o.OrderDetails) // Include order details if needed
                .Include(o => o.Customer)     // Include customer info if needed
                .ToListAsync();
        }
        public async Task<Order?> GetByIdAsync(Guid id)
        {
            return await _dbSet
                .Include(o => o.Customer) // Include Customer data
                .Include(o => o.OrderDetails) // Include OrderDetails if needed
                .FirstOrDefaultAsync(o => o.OrderId == id);
        }
    }

}