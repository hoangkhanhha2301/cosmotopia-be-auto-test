using Cosmetics.Interfaces;
using Cosmetics.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace Cosmetics.Repositories.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IOrderRepository Orders { get; }
        Task<int> CompleteAsync();

        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitAsync(IDbContextTransaction transaction);
        Task RollbackAsync(IDbContextTransaction transaction);
        IOrderDetailRepository OrderDetails { get; }
        IBrandRepository Brands { get; }
        ICategoryRepository Categories { get; }
        IProductRepository Products { get; }
        IUserRepository Users { get; }
        IAffiliateProfileRepository AffiliateProfiles { get; }
        IPaymentTransactionRepository PaymentTransactions { get; }
        ICartDetailRepository CartDetails { get; }



        //IAffiliateLinkRepository AffiliateLinks { get; }
    }

}