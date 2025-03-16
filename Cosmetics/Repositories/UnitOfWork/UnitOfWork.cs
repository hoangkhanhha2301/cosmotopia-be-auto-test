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
        //public IAffiliateLinkRepository AffiliateLinks { get; } // Add this

        public UnitOfWork(
            ComedicShopDBContext context,
            IOrderRepository orderRepository,
            IOrderDetailRepository orderDetailRepository,
            IBrandRepository brandRepository,
            ICategoryRepository categoryRepository,
            IProductRepository productRepository,
            IUserRepository userRepository,
            IAffiliateProfileRepository affiliateProfileRepository
            //IAffiliateLinkRepository affiliateLinkRepository) // Add this
            )
        {
            _context = context;
            Orders = orderRepository;
            OrderDetails = orderDetailRepository;
            Brands = brandRepository;
            Categories = categoryRepository;
            Products = productRepository;
            Users = userRepository; // Assign this
            //AffiliateProfiles = affiliateProfileRepository; // Assign this
            //AffiliateLinks = affiliateLinkRepository; // Assign this
        }

        public async Task<int> CompleteAsync() => await _context.SaveChangesAsync();

        public void Dispose() => _context.Dispose();
    }

}