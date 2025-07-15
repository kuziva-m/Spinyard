namespace Inventory.Core.Application.DTOs
{
    public class ProductVariantDto
    {
        public string Sku { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        // --- This is the required change ---
        public List<string> Variation { get; set; } = new List<string>();
        public string? ImagePath { get; set; }
        public string? ThumbnailImagePath { get; set; }
    }
}