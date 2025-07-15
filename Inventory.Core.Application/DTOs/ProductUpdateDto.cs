namespace Inventory.Core.Application.DTOs
{
    public class ProductUpdateDto
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string? ImagePath { get; set; }
        public List<ProductVariantDto> Variants { get; set; } = new List<ProductVariantDto>();
        public string? OptionNames { get; set; }
    }
}