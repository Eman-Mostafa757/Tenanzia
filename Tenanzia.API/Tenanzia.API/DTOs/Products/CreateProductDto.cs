using System.ComponentModel.DataAnnotations;

namespace Tenanzia.API.DTOs.Products
{
    public class CreateProductDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required]
        public decimal Price { get; set; }
        public string? Unit { get; set; }
    }
}
