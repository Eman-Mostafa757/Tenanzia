using System.ComponentModel.DataAnnotations;

namespace Tenanzia.API.DTOs.Orders
{
    public class CreateOrderDto
    {
        [Required]
        public int CustomerId { get; set; }

        public string? Notes { get; set; }

        [Required]
        [MinLength(1)]
        public List<CreateOrderItemDto> Items { get; set; } = new();
    }
}
