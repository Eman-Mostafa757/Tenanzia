using Stripe;

namespace Tenanzia.API.Models
{
    public class Plan
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int MaxCustomers { get; set; }
        public int MaxTasks { get; set; }
        public string? StripePriceId { get; set; }

        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}
