using Inventory.Core.Domain.Entities;

namespace Inventory.Core.Domain.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<IEnumerable<Product>> GetAllProductsWithDetailsAsync();
        Task<Product?> GetProductWithDetailsAsync(int id);
        Task<IEnumerable<Product>> SearchProductsAsync(string? searchTerm, int? categoryId);

        Task UpdateProductWithVariantsAsync(Product product);
        Task AddProductWithVariantsAsync(Product product);
    }
}