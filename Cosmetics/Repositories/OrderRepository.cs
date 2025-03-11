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
            return await _dbSet.Where(o => o.CustomerId == customerId).ToListAsync();
        }
    }

}