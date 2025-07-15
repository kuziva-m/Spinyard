using Inventory.Core.Domain.Interfaces;
using Inventory.Infrastructure.Repositories;

namespace Inventory.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly InventoryDbContext _context;
        public IProductRepository Products { get; private set; }
        public ICategoryRepository Categories { get; private set; }

        public UnitOfWork(InventoryDbContext context)
        {
            _context = context;
            Products = new ProductRepository(_context);
            Categories = new CategoryRepository(_context);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}