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
        public IBrandRepository Brands { get; }
        public ICategoryRepository Categories { get; }
        public IProductRepository Products { get; }
        public IUserRepository Users { get; }
        public IAffiliateProfileRepository AffiliateProfiles { get; }
        public IGenericRepository<PaymentTransaction> PaymentTransactions { get; } 

        // Uncomment if you need AffiliateLinks later
        // public IAffiliateLinkRepository AffiliateLinks { get; }

        public UnitOfWork(
            ComedicShopDBContext context,
            IOrderRepository orderRepository,
            IOrderDetailRepository orderDetailRepository,
            IBrandRepository brandRepository,
            ICategoryRepository categoryRepository,
            IProductRepository productRepository,
            IUserRepository userRepository,
            IAffiliateProfileRepository affiliateProfileRepository,
            IGenericRepository<PaymentTransaction> paymentTransactionRepository
            // Uncomment and add if needed
            // IAffiliateLinkRepository affiliateLinkRepository
            )
        {
            _context = context;
            Orders = orderRepository;
            OrderDetails = orderDetailRepository;
            Brands = brandRepository;
            Categories = categoryRepository;
            Products = productRepository;
            Users = userRepository;
            AffiliateProfiles = affiliateProfileRepository; // Fixed: Assigned
            PaymentTransactions = paymentTransactionRepository; // Fixed: Assigned

            // Uncomment if needed
            // AffiliateLinks = affiliateLinkRepository;
        }

        public async Task<int> CompleteAsync() => await _context.SaveChangesAsync();

        public void Dispose() => _context.Dispose();
    }
}