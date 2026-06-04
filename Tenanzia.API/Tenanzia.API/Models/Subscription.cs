namespace Tenanzia.API.Models
{
    public class Subscription
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public int PlanId { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }
        public string? StripeSubscriptionId { get; set; }

        public Tenant Tenant { get; set; } = null!;
        public Plan Plan { get; set; } = null!;
    }
}
