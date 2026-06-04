namespace Tenanzia.API.Models
{
    public class Invoice
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int TenantId { get; set; }
        public string Status { get; set; } = "Unpaid";
        public decimal Amount { get; set; }
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }

        public Order Order { get; set; } = null!;
        public Tenant Tenant { get; set; } = null!;
    }
}
