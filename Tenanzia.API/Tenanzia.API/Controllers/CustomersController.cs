using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Tenanzia.API.DTOs.Customers;
using Tenanzia.API.Interfaces;
using Tenanzia.API.Models;
using Tenanzia.API.Services;
using Customer = Tenanzia.API.Models.Customer;

namespace Tenanzia.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly TenanziaContext _context;
        private readonly ITenantService _tenantService;
        private readonly SubscriptionLimitService _subscriptionLimitService;

        public CustomersController(TenanziaContext context, ITenantService tenantService , SubscriptionLimitService subscriptionLimitService)
        {
            _context = context;
            _tenantService = tenantService;
            _subscriptionLimitService = subscriptionLimitService;
        }

        // GET: api/customers
        [HttpGet("GetAll")]
        public IActionResult GetAll([FromQuery] string? search, [FromQuery] string? status)
        {
            var tenantId = _tenantService.GetTenantId();

            var query = _context.Customers
                .Where(c => c.TenantId == tenantId);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(c =>
                    c.Name.Contains(search) ||
                    (c.Email != null && c.Email.Contains(search)) ||
                    (c.Phone != null && c.Phone.Contains(search)));

            if (!string.IsNullOrEmpty(status))
                query = query.Where(c => c.Status == status);

            var result = query.Select(c => new CustomerResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email,
                Phone = c.Phone,
                Address = c.Address,
                Notes = c.Notes,
                Status = c.Status,
                CreatedAt = c.CreatedAt
            }).ToList();

            return Ok(result);
        }

        // GET: api/customers/5
        [HttpGet("GetById/{id}")]
        public IActionResult GetById(int id)
        {
            var tenantId = _tenantService.GetTenantId();

            var customer = _context.Customers
                .Where(c => c.TenantId == tenantId && c.Id == id)
                .Select(c => new CustomerResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Email = c.Email,
                    Phone = c.Phone,
                    Address = c.Address,
                    Notes = c.Notes,
                    Status = c.Status,
                    CreatedAt = c.CreatedAt
                }).FirstOrDefault();

            if (customer == null)
                return NotFound("Customer not found");

            return Ok(customer);
        }

        // POST: api/customers
        [HttpPost("Create")]
        public IActionResult Create(CreateCustomerDto dto)
        {
            var tenantId = _tenantService.GetTenantId();


            var (canAdd, message) = _subscriptionLimitService.CanAddCustomer(tenantId);
            if (!canAdd)
                return BadRequest(new { error = message, upgradeRequired = true });

            var customer = new Customer
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                Notes = dto.Notes,
                Status = "Active",
                TenantId = tenantId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Customers.Add(customer);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = customer.Id }, new CustomerResponseDto
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                Notes = customer.Notes,
                Status = customer.Status,
                CreatedAt = customer.CreatedAt
            });
        }

        // PUT: api/customers/5
        [HttpPut("Update/{id}")]
        public IActionResult Update(int id, UpdateCustomerDto dto)
        {
            var tenantId = _tenantService.GetTenantId();

            var customer = _context.Customers
                .FirstOrDefault(c => c.TenantId == tenantId && c.Id == id);

            if (customer == null)
                return NotFound("Customer not found");

            if (dto.Name != null) customer.Name = dto.Name;
            if (dto.Email != null) customer.Email = dto.Email;
            if (dto.Phone != null) customer.Phone = dto.Phone;
            if (dto.Address != null) customer.Address = dto.Address;
            if (dto.Notes != null) customer.Notes = dto.Notes;
            if (dto.Status != null) customer.Status = dto.Status;

            _context.SaveChanges();

            return Ok("Customer updated successfully");
        }

        // DELETE: api/customers/5
        [HttpDelete("Delete/{id}")]
        public IActionResult Delete(int id)
        {
            var tenantId = _tenantService.GetTenantId();

            var customer = _context.Customers
                .FirstOrDefault(c => c.TenantId == tenantId && c.Id == id);

            if (customer == null)
                return NotFound("Customer not found");

            _context.Customers.Remove(customer);
            _context.SaveChanges();

            return Ok("Customer deleted successfully");
        }



        [HttpGet("GetProfile/{id}")]
        public IActionResult GetProfile(int id)
        {
            var tenantId = _tenantService.GetTenantId();

            var customer = _context.Customers
                .FirstOrDefault(c => c.Id == id && c.TenantId == tenantId);

            if (customer == null)
                return NotFound("Customer not found");

            // Orders
            var orders = _context.Orders
                .Where(o => o.CustomerId == id && o.TenantId == tenantId)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

            // Invoices
            var invoices = _context.Invoices
                .Where(i => i.TenantId == tenantId)
                .Include(i => i.Order)
                .Where(i => i.Order.CustomerId == id)
                .OrderByDescending(i => i.IssuedAt)
                .ToList();

            // Stats
            var totalSpent = orders
                .Where(o => o.Status == "Completed")
                .Sum(o => o.TotalAmount);

            var daysSinceLastOrder = orders.Any()
                ? (int)(DateTime.UtcNow - orders.First().CreatedAt).TotalDays
                : -1;

            // Value Score
            string valueScore;
            if (totalSpent >= 1000) valueScore = "VIP";
            else if (totalSpent >= 500) valueScore = "Regular";
            else valueScore = "New";

            return Ok(new
            {
                // Info
                customer.Id,
                customer.Name,
                customer.Email,
                customer.Phone,
                customer.Address,
                customer.Notes,
                customer.Status,
                customer.CreatedAt,

                // Stats
                TotalOrders = orders.Count,
                TotalSpent = totalSpent,
                PendingOrders = orders.Count(o => o.Status == "Pending"),
                CompletedOrders = orders.Count(o => o.Status == "Completed"),
                PaidInvoices = invoices.Count(i => i.Status == "Paid"),
                UnpaidInvoices = invoices.Count(i => i.Status == "Unpaid"),
                ValueScore = valueScore,
                DaysSinceLastOrder = daysSinceLastOrder,

                // Orders
                Orders = orders.Select(o => new
                {
                    o.Id,
                    o.Status,
                    o.TotalAmount,
                    o.Notes,
                    o.CreatedAt,
                    Items = o.OrderItems.Select(i => new
                    {
                        i.ProductName,
                        i.Quantity,
                        i.UnitPrice,
                        TotalPrice = i.Quantity * i.UnitPrice
                    }).ToList()
                }).ToList(),

                // Invoices
                Invoices = invoices.Select(i => new
                {
                    i.Id,
                    i.Status,
                    i.Amount,
                    i.IssuedAt,
                    i.PaidAt
                }).ToList()
            });
        }


    }
}
