using System.ComponentModel.DataAnnotations;

namespace Tenanzia.API.DTOs.Invoices
{
    public class CreateInvoiceDto
    {
        [Required]
        public int OrderId { get; set; }
    }
}
