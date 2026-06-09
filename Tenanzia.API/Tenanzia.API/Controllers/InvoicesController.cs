using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tenanzia.API.DTOs.Invoices;
using Tenanzia.API.DTOs.Orders;
using Tenanzia.API.Enums;
using Tenanzia.API.Interfaces;
using Tenanzia.API.Models;
using Tenanzia.API.Services;

namespace Tenanzia.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class InvoicesController : ControllerBase
    {
        private readonly TenanziaContext _context;
        private readonly ITenantService _tenantService;
        private readonly NotificationService _notificationService;

        public InvoicesController(TenanziaContext context, ITenantService tenantService, NotificationService notificationService)
        {
            _context = context;
            _tenantService = tenantService;
            _notificationService = notificationService;
        }

        // GET: api/invoices
        [HttpGet]
        public IActionResult GetAll([FromQuery] string? status)
        {
            var tenantId = _tenantService.GetTenantId();

            var query = _context.Invoices
                .Where(i => i.TenantId == tenantId)
                .Include(i => i.Order)
                    .ThenInclude(o => o.Customer).Include(i => i.Order).ThenInclude(o => o.OrderItems)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(i => i.Status == status);

            var result = query
                .OrderByDescending(i => i.IssuedAt)
                .Select(i => new InvoiceResponseDto
                {
                    Id = i.Id,
                    OrderId = i.OrderId,
                    CustomerName = i.Order.Customer.Name,
                    CustomerId = i.Order.Customer.Id,
                    Status = i.Status,
                    Amount = i.Amount,
                    IssuedAt = i.IssuedAt,
                    PaidAt = i.PaidAt,
                    Items = i.Order.OrderItems.Select(oi => new OrderItemResponseDto
                    {
                        Id = oi.Id,
                        ProductName = oi.ProductName,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.Quantity * oi.UnitPrice
                    }).ToList()
                }).ToList();

            return Ok(result);
        }

        // GET: api/invoices/5
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var tenantId = _tenantService.GetTenantId();

            var invoice = _context.Invoices
                .Where(i => i.TenantId == tenantId && i.Id == id)
                .Include(i => i.Order)
                    .ThenInclude(o => o.Customer)
                    .Include(i => i.Order).ThenInclude(o => o.OrderItems)
                .Select(i => new InvoiceResponseDto
                {
                    Id = i.Id,
                    OrderId = i.OrderId,
                    CustomerName = i.Order.Customer.Name,
                    Status = i.Status,
                    Amount = i.Amount,
                    IssuedAt = i.IssuedAt,
                    PaidAt = i.PaidAt,
                    Items = i.Order.OrderItems.Select(oi => new OrderItemResponseDto
                    {
                        Id = oi.Id,
                        ProductName = oi.ProductName,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.Quantity * oi.UnitPrice
                    }).ToList()

                }).FirstOrDefault();

            if (invoice == null)
                return NotFound("Invoice not found");

            return Ok(invoice);
        }

        // POST: api/invoices
        [HttpPost]
        public IActionResult Create(CreateInvoiceDto dto)
        {
            var tenantId = _tenantService.GetTenantId();

            // تأكد إن الـ Order بتاع نفس الـ tenant
            var order = _context.Orders
                .FirstOrDefault(o => o.Id == dto.OrderId && o.TenantId == tenantId);

            if (order == null)
                return BadRequest("Order not found in this tenant");

            // تأكد مفيش invoice موجودة للـ order ده
            var exists = _context.Invoices
                .Any(i => i.OrderId == dto.OrderId);

            if (exists)
                return BadRequest("Invoice already exists for this order");

            var invoice = new Invoice
            {
                OrderId = dto.OrderId,
                TenantId = tenantId,
                Status = InvoiceStatus.Unpaid,
                Amount = order.TotalAmount,
                IssuedAt = DateTime.UtcNow
            };

            _context.Invoices.Add(invoice);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = invoice.Id }, new InvoiceResponseDto
            {
                Id = invoice.Id,
                OrderId = invoice.OrderId,
                CustomerName = _context.Orders
                    .Include(o => o.Customer)
                    .First(o => o.Id == invoice.OrderId).Customer.Name,
                Status = invoice.Status,
                Amount = invoice.Amount,
                IssuedAt = invoice.IssuedAt,
                PaidAt = invoice.PaidAt,
                Items = _context.Orders
                    .Include(o => o.OrderItems)
                    .First(o => o.Id == invoice.OrderId)
                    .OrderItems.Select(oi => new OrderItemResponseDto
                    {
                        Id = oi.Id,
                        ProductName = oi.ProductName,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.Quantity * oi.UnitPrice
                    }).ToList()
            });
        }

        // PATCH: api/invoices/5/pay
        [HttpPatch("{id}/pay")]
        public IActionResult MarkAsPaid(int id)
        {
            var tenantId = _tenantService.GetTenantId();

            var invoice = _context.Invoices
                .FirstOrDefault(i => i.TenantId == tenantId && i.Id == id);

            if (invoice == null)
                return NotFound("Invoice not found");

            if (invoice.Status == InvoiceStatus.Paid)
                return BadRequest("Invoice is already paid");

            invoice.Status = InvoiceStatus.Paid;
            invoice.PaidAt = DateTime.UtcNow;

            // حدّث الـ Order كمان
            var order = _context.Orders.FirstOrDefault(o => o.Id == invoice.OrderId);
            if (order != null)
                order.Status = OrderStatus.Completed;

            _context.SaveChanges();
            // في MarkAsPaid
            var owners = _context.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.Role.Name == "Owner")
                .Join(_context.UserTenants.Where(ut => ut.TenantId == tenantId),
                      ur => ur.UserId, ut => ut.UserId, (ur, ut) => ur.UserId)
                .ToList();

            foreach (var ownerId in owners)
            {
                _notificationService.Create(
                    tenantId,
                    ownerId ??0,
                    "Invoice Paid! 💰",
                    $"Invoice #{id} has been paid — ${invoice.Amount:N0}",
                    "invoice"
                );
            }

            return Ok("Invoice marked as paid");
        }

        // PATCH: api/invoices/5/cancel
        [HttpPatch("{id}/cancel")]
        public IActionResult Cancel(int id)
        {
            var tenantId = _tenantService.GetTenantId();

            var invoice = _context.Invoices
                .FirstOrDefault(i => i.TenantId == tenantId && i.Id == id);

            if (invoice == null)
                return NotFound("Invoice not found");

            if (invoice.Status == InvoiceStatus.Paid)
                return BadRequest("Cannot cancel a paid invoice");

            invoice.Status = InvoiceStatus.Cancelled;
            _context.SaveChanges();

            return Ok("Invoice cancelled");
        }

        // DELETE: api/invoices/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var tenantId = _tenantService.GetTenantId();

            var invoice = _context.Invoices
                .FirstOrDefault(i => i.TenantId == tenantId && i.Id == id);

            if (invoice == null)
                return NotFound("Invoice not found");

            if (invoice.Status == InvoiceStatus.Paid)
                return BadRequest("Cannot delete a paid invoice");

            _context.Invoices.Remove(invoice);
            _context.SaveChanges();

            return Ok("Invoice deleted");
        }

        [HttpPost("{id}/send")]
        public async Task<IActionResult> SendInvoice(int id, [FromServices] EmailService emailService)
        {
            var tenantId = _tenantService.GetTenantId();

            var invoice = _context.Invoices
                .Include(i => i.Order)
                    .ThenInclude(o => o.Customer)
                .Include(i => i.Order)
                    .ThenInclude(o => o.OrderItems) // ← جديد
                .Include(i => i.Tenant)
                .FirstOrDefault(i => i.Id == id && i.TenantId == tenantId);

            if (invoice == null)
                return NotFound("Invoice not found");

            var customer = invoice.Order.Customer;

            if (string.IsNullOrEmpty(customer.Email))
                return BadRequest("Customer has no email address");

            // جيبي الـ items وبعتيهم
            var items = invoice.Order.OrderItems.Select(i => new
            {
                i.ProductName,
                i.Quantity,
                i.UnitPrice,
                TotalPrice = i.Quantity * i.UnitPrice
            }).ToList();

            await emailService.SendInvoiceEmail(
                toEmail: customer.Email,
                toName: customer.Name,
                invoiceId: invoice.Id,
                orderId: invoice.OrderId,
                amount: invoice.Amount,
                status: invoice.Status,
                companyName: invoice.Tenant.Name,
                items: items.Select(i => (i.ProductName, i.Quantity, i.UnitPrice, i.TotalPrice)).ToList()
            );

            return Ok("Invoice sent successfully");
        }
    }
}
