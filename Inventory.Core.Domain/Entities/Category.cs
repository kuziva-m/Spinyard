namespace Inventory.Core.Domain.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Fix: Initialize to empty string
        public int? ParentCategoryId { get; set; }
        public virtual Category? ParentCategory { get; set; } // Fix: Make nullable
        public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}