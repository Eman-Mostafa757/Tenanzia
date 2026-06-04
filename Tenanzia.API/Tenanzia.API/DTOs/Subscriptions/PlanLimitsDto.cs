namespace Tenanzia.API.DTOs.Subscriptions
{
    public class PlanLimitsDto
    {
        public string PlanName { get; set; } = string.Empty;
        public int MaxCustomers { get; set; }
        public int CurrentCustomers { get; set; }
        public int MaxTasks { get; set; }
        public int CurrentTasks { get; set; }
        public bool IsUnlimited { get; set; }
    }
}
