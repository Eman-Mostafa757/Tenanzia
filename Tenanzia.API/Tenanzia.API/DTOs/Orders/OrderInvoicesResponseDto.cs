namespace Tenanzia.API.DTOs.Orders
{
    public class OrderInvoicesResponseDto
    {
        public int Id { get; set; }
        public string Status { get; set; } = "Unpaid";
        public decimal Amount { get; set; }
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }

    }
}
