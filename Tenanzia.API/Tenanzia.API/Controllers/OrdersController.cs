using Microsoft.AspNetCore.Authorization;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tenanzia.API.DTOs.Orders;
using Tenanzia.API.Enums;
using Tenanzia.API.Interfaces;
using Tenanzia.API.Models;
using Microsoft.EntityFrameworkCore;
using Tenanzia.API.Services;

namespace Tenanzia.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly TenanziaContext _context;
        private readonly ITenantService _tenantService;
        private readonly NotificationService _notificationService;
        public OrdersController(TenanziaContext context, ITenantService tenantService, NotificationService notificationService)
        {
            _context = context;
            _tenantService = tenantService;
            _notificationService = notificationService;
        }

        // GET: api/orders
        [HttpGet]
        public IActionResult GetAll([FromQuery] string? status)
        {
            var tenantId = _tenantService.GetTenantId();

            var query = _context.Orders
                .Where(o => o.TenantId == tenantId)
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(o => o.Status == status);

            var result = query.Select(o => new OrderResponseDto
            {
                Id = o.Id,
                CustomerId = o.CustomerId,
                CustomerName = o.Customer.Name,
                Status = o.Status,
                TotalAmount = o.TotalAmount,
                Notes = o.Notes,
                CreatedAt = o.CreatedAt,
                Items = o.OrderItems.Select(i => new OrderItemResponseDto
                {
                    Id = i.Id,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.Quantity * i.UnitPrice
                }).ToList(),
                Invoices = _context.Invoices.Where(inv => inv.OrderId == o.Id)
                    .Select(inv => new OrderInvoicesResponseDto
                    {
                        Id = inv.Id,
                        Status = inv.Status,
                        Amount = inv.Amount,
                        IssuedAt = inv.IssuedAt,
                        PaidAt = inv.PaidAt


                    }).FirstOrDefault()
            }).ToList();
            return Ok(result);
        }

        // GET: api/orders/5
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var tenantId = _tenantService.GetTenantId();

            var order = _context.Orders
                .Where(o => o.TenantId == tenantId && o.Id == id)
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .Select(o => new OrderResponseDto
                {
                    Id = o.Id,
                    CustomerId = o.CustomerId,
                    CustomerName = o.Customer.Name,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount,
                    Notes = o.Notes,
                    CreatedAt = o.CreatedAt,
                    Items = o.OrderItems.Select(i => new OrderItemResponseDto
                    {
                        Id = i.Id,
                        ProductName = i.ProductName,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        TotalPrice = i.Quantity * i.UnitPrice
                    }).ToList()
                }).FirstOrDefault();

            if (order == null)
                return NotFound("Order not found");

            return Ok(order);
        }

        // POST: api/orders
        [HttpPost]
        public IActionResult Create(CreateOrderDto dto)
        {
            var tenantId = _tenantService.GetTenantId();

            // تأكد إن الـ Customer بتاع نفس الـ tenant
            var customer = _context.Customers
                .FirstOrDefault(c => c.Id == dto.CustomerId && c.TenantId == tenantId);

            if (customer == null)
                return BadRequest("Customer not found in this tenant");

            // احسب الـ Total
            var total = dto.Items.Sum(i => i.Quantity * i.UnitPrice);

            var order = new Order
            {
                CustomerId = dto.CustomerId,
                TenantId = tenantId,
                Status = OrderStatus.Pending,
                TotalAmount = total,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow,
                OrderItems = dto.Items.Select(i => new OrderItem
                {
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                }).ToList()
            };
            foreach (var item in dto.Items)
            {
                var product = _context.Products.FirstOrDefault(p => p.Name == item.ProductName && p.TenantId == tenantId && p.TrackStock);
                if (product != null && product.StockQuantity < item.Quantity)
                {
                    return BadRequest($"Not enough stock for product '{item.ProductName}'. Available: {product.StockQuantity}");
                }
            }


                _context.Orders.Add(order);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = order.Id }, new OrderResponseDto
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                CustomerName = customer.Name,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                Notes = order.Notes,
                CreatedAt = order.CreatedAt,
                Items = order.OrderItems.Select(i => new OrderItemResponseDto
                {
                    Id = i.Id,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.Quantity * i.UnitPrice
                }).ToList()
            });
        }

        // PATCH: api/orders/5/status
        [HttpPatch("{id}/status")]
        public IActionResult UpdateStatus(int id, [FromBody] string newStatus)
        {
            var tenantId = _tenantService.GetTenantId();

            var validStatuses = new[]
            {
        OrderStatus.Pending,
        OrderStatus.Processing,
        OrderStatus.Completed,
        OrderStatus.Cancelled
            };

            if (!validStatuses.Contains(newStatus))
                return BadRequest("Invalid status value");

            var order = _context.Orders.Include(o=> o.OrderItems)
                .FirstOrDefault(o => o.TenantId == tenantId && o.Id == id);


            var owners = _context.UserRoles
                            .Include(ur => ur.Role)
                            .Where(ur => ur.Role.Name == "Owner" || ur.Role.Name == "Manager")
                            .Join(_context.UserTenants.Where(ut => ut.TenantId == tenantId),
                                  ur => ur.UserId, ut => ut.UserId, (ur, ut) => (int)ur.UserId) // ← cast لـ int
                            .ToList();


            if (order == null)
                return NotFound("Order not found");

            var oldStatus = order.Status;
            order.Status = newStatus;

            // لما يوصل Processing → اعمل Invoice تلقائياً
            // لما يوصل Processing → اعمل Invoice وخصم الـ Stock
            if (newStatus == "Processing" && oldStatus != "Processing")
            {
                // ✅ Check Stock Availability الأول
                foreach (var item in order.OrderItems)
                {
                    var product = _context.Products
                        .FirstOrDefault(p => p.Name == item.ProductName &&
                                             p.TenantId == tenantId &&
                                             p.TrackStock);

                    if (product != null && product.StockQuantity < item.Quantity)
                    {
                        return BadRequest(new
                        {
                            error = $"Not enough stock for '{product.Name}'. Available: {product.StockQuantity}, Required: {item.Quantity}"
                        });
                    }
                }

                // Invoice
                var exists = _context.Invoices.Any(i => i.OrderId == id);
                if (!exists)
                {
                    _context.Invoices.Add(new Invoice
                    {
                        OrderId = id,
                        TenantId = tenantId,
                        Status = "Unpaid",
                        Amount = order.TotalAmount,
                        IssuedAt = DateTime.UtcNow
                    });
                }

                // خصم الـ Stock بعد التأكد
                foreach (var item in order.OrderItems)
                {
                    var product = _context.Products
                        .FirstOrDefault(p => p.Name == item.ProductName &&
                                             p.TenantId == tenantId &&
                                             p.TrackStock);

                    if (product != null)
                    {
                        product.StockQuantity -= item.Quantity;

                        // Low Stock Notification
                        if (product.StockQuantity <= product.LowStockThreshold)
                        {

                            foreach (var ownerId in owners)
                            {
                                _notificationService.Create(
                                    tenantId,
                                    ownerId,
                                    "⚠️ Low Stock Alert",
                                    $"{product.Name} is running low — only {product.StockQuantity} left",
                                    "stock"
                                );
                            }
                        }
                    }
                }
            }
            // لما Invoice تتدفع → Order يبقى Completed تلقائياً
            if (newStatus == OrderStatus.Completed)
            {
                var invoice = _context.Invoices.FirstOrDefault(i => i.OrderId == id);
                if (invoice != null && invoice.Status != "Paid")
                {
                    invoice.Status = "Paid";
                    invoice.PaidAt = DateTime.UtcNow;
                }
            }
            _context.SaveChanges();

            // في UpdateStatus — notify الـ Owner
            foreach (var ownerId in owners)
            {
                _notificationService.Create(
                    tenantId,
                    ownerId,
                    "Order Status Updated",
                    $"Order #{id} is now {newStatus}",
                    "order"
                );
            }


            if (newStatus == "Cancelled" && oldStatus == "Processing")
            {
                foreach (var item in order.OrderItems)
                {
                    var product = _context.Products
                        .FirstOrDefault(p => p.Name == item.ProductName &&
                                             p.TenantId == tenantId &&
                                             p.TrackStock);

                    if (product != null)
                        product.StockQuantity += item.Quantity;
                }
            }
            return Ok(new
            {
                message = "Status updated successfully"
            });


        }

        // DELETE: api/orders/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var tenantId = _tenantService.GetTenantId();

            var order = _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.TenantId == tenantId && o.Id == id);

            if (order == null)
                return NotFound("Order not found");

            _context.Orders.Remove(order);
            _context.SaveChanges();

            return Ok("Order deleted successfully");
        }

        [HttpGet("stats")]
        public IActionResult GetStats()
        {
            var tenantId = _tenantService.GetTenantId();

            var orders = _context.Orders
                .Where(o => o.TenantId == tenantId)
                .ToList();

            return Ok(new
            {
                Total = orders.Count,
                Pending = orders.Count(o => o.Status == OrderStatus.Pending),
                Processing = orders.Count(o => o.Status == OrderStatus.Processing),
                Completed = orders.Count(o => o.Status == OrderStatus.Completed),
                Cancelled = orders.Count(o => o.Status == OrderStatus.Cancelled),
                TotalRevenue = orders
                    .Where(o => o.Status == OrderStatus.Completed)
                    .Sum(o => o.TotalAmount)
            });
        }
    }
}
