using System.ComponentModel.DataAnnotations;

namespace Tenanzia.API.DTOs.Orders
{
    public class CreateOrderItemDto
    {
        [Required]
        [MaxLength(100)]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal UnitPrice { get; set; }
    }
}
