using Cosmetics.Interfaces;

namespace Cosmetics.Repositories.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IOrderRepository Orders { get; }
        Task<int> CompleteAsync();
        IOrderDetailRepository OrderDetails { get; }
        IBrandRepository Brands { get; }
        ICategoryRepository Categories { get; }
        IProductRepository Products { get; }
        IUserRepository Users { get; }
        IAffiliateProfileRepository AffiliateProfiles { get; }
        //IAffiliateLinkRepository AffiliateLinks { get; }
    }

}