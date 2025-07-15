// In CategoryRepository.cs
using Inventory.Core.Domain.Entities;
using Inventory.Core.Domain.Interfaces;
using Inventory.Infrastructure.Data;

namespace Inventory.Infrastructure.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(InventoryDbContext context) : base(context) { }
    }
}