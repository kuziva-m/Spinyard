// In Inventory.Core.Domain/Interfaces/ICategoryRepository.cs
using Inventory.Core.Domain.Entities;

namespace Inventory.Core.Domain.Interfaces
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        // We can add methods specific to Categories here later.
    }
}