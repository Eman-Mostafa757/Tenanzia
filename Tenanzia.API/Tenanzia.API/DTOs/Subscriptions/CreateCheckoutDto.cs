using System.ComponentModel.DataAnnotations;

namespace Tenanzia.API.DTOs.Subscriptions
{
    public class CreateCheckoutDto
    {
        [Required]
        public string PlanName { get; set; } = string.Empty; // "Pro" or "Enterprise"
    }
}
