namespace Tenanzia.API.Models
{
    public class Product
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? Unit { get; set; }
        public bool IsActive { get; set; } = true;
        public int StockQuantity { get; set; } = 0;      // ← جديد
        public int LowStockThreshold { get; set; } = 5;  // ← جديد
        public bool TrackStock { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Tenant Tenant { get; set; } = null!;
    }
}
