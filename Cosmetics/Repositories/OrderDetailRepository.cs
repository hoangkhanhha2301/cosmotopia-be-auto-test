using Cosmetics.Interfaces;
using Cosmetics.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace Cosmetics.Repositories
{
    public class OrderDetailRepository : GenericRepository<OrderDetail>, IOrderDetailRepository
    {
        public OrderDetailRepository(ComedicShopDBContext context) : base(context) { }

        public async Task<IEnumerable<OrderDetail>> GetByOrderIdAsync(Guid orderId)
        {
            return await _context.OrderDetails
                .Where(od => od.OrderId == orderId)
                .ToListAsync();
        }
    }

}