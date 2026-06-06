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
        public int StockQuantity { get; set; } = 0;
        public int LowStockThreshold { get; set; } = 5;
        public bool TrackStock { get; set; } = true;
    }
}
