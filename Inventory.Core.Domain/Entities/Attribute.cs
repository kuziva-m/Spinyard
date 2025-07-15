namespace Inventory.Core.Domain.Entities
{
    public class Attribute
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Fix: Initialize to empty string
    }
}