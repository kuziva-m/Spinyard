namespace Inventory.Core.Application.DTOs
{
    public class ProductCreateDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string? ImagePath { get; set; }
        public List<ProductVariantCreateDto> Variants { get; set; } = new List<ProductVariantCreateDto>();
        public string? OptionNames { get; set; }
    }
}