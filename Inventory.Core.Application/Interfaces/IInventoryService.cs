using Inventory.Core.Application.DTOs;

namespace Inventory.Core.Application.Interfaces
{
    public interface IInventoryService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();

        Task CreateProductAsync(ProductCreateDto productDto);

        Task<IEnumerable<CategoryDto>> GetCategoriesAsync();

        Task UpdateProductAsync(ProductUpdateDto productDto);

        Task DeleteProductAsync(int productId);

        Task<IEnumerable<ProductDto>> SearchProductsAsync(string? searchTerm, int? categoryId);

        Task AddCategoryAsync(CategoryCreateDto categoryDto);

        Task UpdateCategoryAsync(CategoryUpdateDto categoryDto);

        Task DeleteCategoryAsync(int categoryId);
    }
}