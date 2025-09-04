using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Inventory.Core.Application.DTOs;
using Inventory.Core.Application.Interfaces;
using Inventory.Core.Domain.Entities;
using Inventory.Core.Domain.Interfaces;

namespace Inventory.Core.Application.Services
{
    public class InventoryService(IUnitOfWork unitOfWork) : IInventoryService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task CreateProductAsync(ProductCreateDto productDto)
        {
            var newProduct = new Product
            {
                Name = productDto.ProductName,
                CategoryId = productDto.CategoryId,
                OptionNames = productDto.OptionNames
            };

            string? finalImagePath = null;
            if (!string.IsNullOrEmpty(productDto.ImagePath))
            {
                var imageDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                Directory.CreateDirectory(imageDir);
                var fileExtension = Path.GetExtension(productDto.ImagePath);
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                finalImagePath = Path.Combine(imageDir, uniqueFileName);
                File.Copy(productDto.ImagePath, finalImagePath);
            }

            foreach (var variantDto in productDto.Variants)
            {
                var optionNames = productDto.OptionNames?.Split(',') ?? [];

                newProduct.Variants.Add(new ProductVariant
                {
                    SKU = variantDto.Sku,
                    Quantity = variantDto.Quantity,
                    Price = variantDto.Price,
                    ImagePath = finalImagePath,
                    ThumbnailImagePath = finalImagePath,
                    AttributeOptions = [
                        .. variantDto.Variation
                            .Select((optionValue, i) => new AttributeOption
                            {
                                Attribute = new Domain.Entities.Attribute
                                {
                                    Name = i < optionNames.Length ? optionNames[i].Trim() : $"Option {i + 1}"
                                },
                                Value = optionValue
                            })
                    ]
                });
            }

            await _unitOfWork.Products.AddProductWithVariantsAsync(newProduct);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _unitOfWork.Products.GetAllProductsWithDetailsAsync();
            return products.Select(MapProductToDto);
        }

        public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync()
        {
            var categories = await _unitOfWork.Categories.GetAllAsync();
            return categories.Select(c => new CategoryDto { Id = c.Id, Name = c.Name });
        }

        public async Task UpdateProductAsync(ProductUpdateDto productDto)
        {
            // Map the DTO to a domain object first
            var product = new Product
            {
                Id = productDto.Id,
                Name = productDto.ProductName,
                CategoryId = productDto.CategoryId,
                OptionNames = productDto.OptionNames
            };

            string? finalImagePath = null;
            if (!string.IsNullOrEmpty(productDto.ImagePath))
            {
                var imageDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                Directory.CreateDirectory(imageDir);
                var fileExtension = Path.GetExtension(productDto.ImagePath);
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                finalImagePath = Path.Combine(imageDir, uniqueFileName);
                File.Copy(productDto.ImagePath, finalImagePath, true);
            }

            foreach (var variantDto in productDto.Variants)
            {
                var optionNames = productDto.OptionNames?.Split(',') ?? [];

                product.Variants.Add(new ProductVariant
                {
                    SKU = variantDto.Sku,
                    Quantity = variantDto.Quantity,
                    Price = variantDto.Price,
                    ImagePath = finalImagePath,
                    ThumbnailImagePath = finalImagePath,
                    AttributeOptions = [
                        .. variantDto.Variation
                            .Select((optionValue, i) => new AttributeOption
                            {
                                Attribute = new Domain.Entities.Attribute
                                {
                                    Name = i < optionNames.Length ? optionNames[i].Trim() : $"Option {i + 1}"
                                },
                                Value = optionValue
                            })
                    ]
                });
            }

            // Call the new repository method
            await _unitOfWork.Products.UpdateProductWithVariantsAsync(product);

            // Commit the transaction
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteProductAsync(int productId)
        {
            var productToDelete = await _unitOfWork.Products.GetProductWithDetailsAsync(productId);
            if (productToDelete == null) return;

            foreach (var variant in productToDelete.Variants)
            {
                if (!string.IsNullOrEmpty(variant.ImagePath) && File.Exists(variant.ImagePath))
                {
                    File.Delete(variant.ImagePath);
                }
                if (!string.IsNullOrEmpty(variant.ThumbnailImagePath) && File.Exists(variant.ThumbnailImagePath))
                {
                    File.Delete(variant.ThumbnailImagePath);
                }
            }

            _unitOfWork.Products.Remove(productToDelete);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string? searchTerm, int? categoryId)
        {
            var products = await _unitOfWork.Products.SearchProductsAsync(searchTerm, categoryId);
            return products.Select(MapProductToDto);
        }

        public async Task AddCategoryAsync(CategoryCreateDto categoryDto)
        {
            var newCategory = new Category
            {
                Name = categoryDto.Name
            };
            await _unitOfWork.Categories.AddAsync(newCategory);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateCategoryAsync(CategoryUpdateDto categoryDto)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(categoryDto.Id);
            if (category != null)
            {
                category.Name = categoryDto.Name;
                _unitOfWork.Categories.Update(category);
                await _unitOfWork.CompleteAsync();
            }
        }

        public async Task DeleteCategoryAsync(int categoryId)
        {
            // Check if any product is using this category
            var products = await _unitOfWork.Products.GetAllAsync();
            if (products.Any(p => p.CategoryId == categoryId))
            {
                throw new InvalidOperationException("This category cannot be deleted because it is currently in use by one or more products.");
            }

            var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);
            if (category != null)
            {
                _unitOfWork.Categories.Remove(category);
                await _unitOfWork.CompleteAsync();
            }
        }

        private ProductDto MapProductToDto(Product product)
        {
            var variants = product.Variants.Select(v => new ProductVariantDto
            {
                Sku = v.SKU ?? string.Empty,
                Quantity = v.Quantity,
                Price = v.Price ?? 0,
                ImagePath = v.ImagePath,
                ThumbnailImagePath = v.ThumbnailImagePath,
                Variation = [.. v.AttributeOptions.Select(ao => ao.Value)]
            }).ToList();

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                CategoryName = product.Category?.Name ?? "N/A",
                Variants = new ObservableCollection<ProductVariantDto>(variants),
                OptionNames = product.OptionNames,
                VariantsDisplay = string.Join(", ",
                    variants.Select(v => string.Join(" / ", v.Variation))
                            .Where(s => !string.IsNullOrEmpty(s))
                            .Take(3))
            };
        }
    }
}
