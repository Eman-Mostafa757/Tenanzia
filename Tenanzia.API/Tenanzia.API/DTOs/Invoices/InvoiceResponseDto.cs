using Tenanzia.API.DTOs.Orders;
using Tenanzia.API.Models;

namespace Tenanzia.API.DTOs.Invoices
{
    public class InvoiceResponseDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime IssuedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public List<OrderItemResponseDto> Items { get; set; } = new();

    }
}
