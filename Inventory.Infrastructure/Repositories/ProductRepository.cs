using Inventory.Core.Domain.Entities;
using Inventory.Core.Domain.Interfaces;
using Inventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using DomainAttribute = Inventory.Core.Domain.Entities.Attribute;

namespace Inventory.Infrastructure.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(InventoryDbContext context) : base(context) { }

        public async Task AddProductWithVariantsAsync(Product product)
        {
            foreach (var variant in product.Variants)
            {
                var processedOptions = new List<AttributeOption>();
                foreach (var option in variant.AttributeOptions)
                {
                    // This null check fixes the compiler warning
                    if (option.Attribute == null) continue;

                    var attr = await _context.Attributes.FirstOrDefaultAsync(a => a.Name == option.Attribute.Name);
                    if (attr == null)
                    {
                        attr = new DomainAttribute { Name = option.Attribute.Name };
                        _context.Attributes.Add(attr);
                    }

                    var attrOption = await _context.AttributeOptions.FirstOrDefaultAsync(o => o.Attribute != null && o.Attribute.Name == attr.Name && o.Value == option.Value);
                    if (attrOption == null)
                    {
                        attrOption = new AttributeOption { Attribute = attr, Value = option.Value };
                        _context.AttributeOptions.Add(attrOption);
                    }
                    processedOptions.Add(attrOption);
                }
                variant.AttributeOptions = processedOptions;
            }
            await _dbSet.AddAsync(product);
        }

        public async Task UpdateProductWithVariantsAsync(Product product)
        {
            var existingProduct = await _dbSet
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == product.Id);

            if (existingProduct == null)
            {
                return; // Or throw an exception
            }

            // Update the product's scalar properties
            _context.Entry(existingProduct).CurrentValues.SetValues(product);
            existingProduct.OptionNames = product.OptionNames;

            // Remove old variants
            _context.ProductVariants.RemoveRange(existingProduct.Variants);

            // Add the new/updated variants from the incoming product object
            foreach (var variant in product.Variants)
            {
                var processedOptions = new List<AttributeOption>();
                foreach (var option in variant.AttributeOptions)
                {
                    if (option.Attribute == null) continue;

                    var attr = await _context.Attributes.FirstOrDefaultAsync(a => a.Name == option.Attribute.Name);
                    if (attr == null)
                    {
                        attr = new DomainAttribute { Name = option.Attribute.Name };
                        _context.Attributes.Add(attr);
                    }

                    var attrOption = await _context.AttributeOptions
                        .FirstOrDefaultAsync(o => o.Attribute != null && o.Attribute.Name == attr.Name && o.Value == option.Value);

                    if (attrOption == null)
                    {
                        attrOption = new AttributeOption { Attribute = attr, Value = option.Value };
                        _context.AttributeOptions.Add(attrOption);
                    }
                    processedOptions.Add(attrOption);
                }
                variant.AttributeOptions = processedOptions;
                existingProduct.Variants.Add(variant);
            }
        }

        public async Task<IEnumerable<Product>> GetAllProductsWithDetailsAsync()
        {
            return await _dbSet
              .Include(p => p.Category)
              .Include(p => p.Variants)
                .ThenInclude(v => v.AttributeOptions)
                    .ThenInclude(ao => ao.Attribute)
              .ToListAsync();
        }

        public async Task<Product?> GetProductWithDetailsAsync(int id)
        {
            return await _dbSet
               .Include(p => p.Category)
               .Include(p => p.Variants)
                 .ThenInclude(v => v.AttributeOptions)
                    .ThenInclude(ao => ao.Attribute)
               .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string? searchTerm, int? categoryId)
        {
            var query = _dbSet
               .Include(p => p.Category)
               .Include(p => p.Variants)
                 .ThenInclude(v => v.AttributeOptions)
                    .ThenInclude(ao => ao.Attribute)
               .AsQueryable();

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(term) || p.Variants.Any(v => v.SKU != null && v.SKU.ToLower().Contains(term)));
            }

            return await query.ToListAsync();
        }
    }
}