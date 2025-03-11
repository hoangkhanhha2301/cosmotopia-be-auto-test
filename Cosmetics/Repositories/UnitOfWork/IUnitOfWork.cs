using Cosmetics.Interfaces;

namespace Cosmetics.Repositories.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IOrderRepository Orders { get; }
        Task<int> CompleteAsync();
        IOrderDetailRepository OrderDetails { get; }
    }

}