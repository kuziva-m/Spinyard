namespace Inventory.Core.Application.DTOs
{
    public class ProductVariantCreateDto
    {
        public string Sku { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public List<string> Variation { get; set; } = new List<string>();
    }
}