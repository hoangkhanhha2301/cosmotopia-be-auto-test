using Cosmetics.Interfaces;
using Cosmetics.Models;
using Microsoft.EntityFrameworkCore;

namespace Cosmetics.Repositories.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ComedicShopDBContext _context;
        public IOrderRepository Orders { get; }
        public IOrderDetailRepository OrderDetails { get; }

        public UnitOfWork(ComedicShopDBContext context, IOrderRepository orderRepository, IOrderDetailRepository orderDetailRepository)
        {
            _context = context;
            Orders = orderRepository;
            OrderDetails = orderDetailRepository;
        }

        public async Task<int> CompleteAsync() => await _context.SaveChangesAsync();

        public void Dispose() => _context.Dispose();
    }

}
