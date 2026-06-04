using System.ComponentModel.DataAnnotations;

namespace Tenanzia.API.DTOs.Customers
{
    public class CreateCustomerDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [EmailAddress]
        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
