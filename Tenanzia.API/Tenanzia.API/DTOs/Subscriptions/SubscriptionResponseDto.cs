namespace Tenanzia.API.DTOs.Subscriptions
{
    public class SubscriptionResponseDto
    {
        public int Id { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
