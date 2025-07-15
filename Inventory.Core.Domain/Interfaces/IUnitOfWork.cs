// In Inventory.Core.Domain/Interfaces/IUnitOfWork.cs
namespace Inventory.Core.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IProductRepository Products { get; }
        ICategoryRepository Categories { get; }

        Task<int> CompleteAsync();
    }
}