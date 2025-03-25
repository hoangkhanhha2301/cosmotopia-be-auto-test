using Cosmetics.Interfaces;
using Cosmetics.Models;
using Cosmetics.Repositories.Interface;
using Microsoft.EntityFrameworkCore.Storage;

namespace Cosmetics.Repositories.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IOrderRepository Orders { get; }
        IOrderDetailRepository OrderDetails { get; }
        IBrandRepository Brands { get; }
        ICategoryRepository Categories { get; }
        IProductRepository Products { get; }
        IUserRepository Users { get; }
        IPaymentTransactionRepository PaymentTransactions { get; }
        ICartDetailRepository CartDetails { get; }



        // Thêm các repository liên quan đến Affiliate
        IAffiliateRepository Affiliates { get; }


        Task<int> CompleteAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitAsync(IDbContextTransaction transaction);
        Task RollbackAsync(IDbContextTransaction transaction);
    }
}