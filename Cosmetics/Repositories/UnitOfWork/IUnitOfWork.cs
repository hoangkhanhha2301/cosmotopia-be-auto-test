using Cosmetics.Interfaces;

namespace Cosmetics.Repositories.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IOrderRepository Orders { get; }
        IOrderDetailRepository OrderDetails { get; }
        Task<int> CompleteAsync();
    }

}
