namespace Inventory.Core.Domain.Entities
{
    public class AttributeOption
    {
        public int Id { get; set; }
        public int AttributeId { get; set; }
        public string Value { get; set; } = string.Empty; // Fix: Initialize to empty string
        public virtual Attribute? Attribute { get; set; } // Fix: Make nullable
    }
}