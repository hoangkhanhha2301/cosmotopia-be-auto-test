using Cosmetics.Enum;
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
                .Include(o => o.OrderDetails).ThenInclude(od => od.Product)   // Include order details if needed
                .Include(o => o.Customer) // Include customer info if needed
                .ToListAsync();
        }
        public async Task<Order?> GetByIdAsync(Guid id, string includeProperties = null)
        {
            var query = _context.Orders.AsQueryable();

            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }

            return await query.FirstOrDefaultAsync(o => o.OrderId == id);
        }
        public async Task<IEnumerable<Order>> GetConfirmedPaidOrdersForShipperAsync(int page, int pageSize)
        {
            return await GetAsync(
                filter: o => o.Status == OrderStatus.Pending
                          && _context.PaymentTransactions.Any(pt => pt.OrderId == o.OrderId && pt.Status == PaymentStatus.Success),
                includeOperations: new Func<IQueryable<Order>, IQueryable<Order>>[]
                {
                    q => q.Include(o => o.OrderDetails).ThenInclude(od => od.Product),
                    q => q.Include(o => o.Customer)
                },
                page: page,
                pageSize: pageSize
            );
        }
    
    }

}