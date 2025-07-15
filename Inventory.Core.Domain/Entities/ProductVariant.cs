namespace Inventory.Core.Domain.Entities
{
    public class ProductVariant
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? SKU { get; set; }
        public int Quantity { get; set; }
        public decimal? Price { get; set; }
        public string? ImagePath { get; set; }
        public string? ThumbnailImagePath { get; set; }
        public virtual Product? Product { get; set; }
        public virtual ICollection<AttributeOption> AttributeOptions { get; set; } = new List<AttributeOption>();
    }
}