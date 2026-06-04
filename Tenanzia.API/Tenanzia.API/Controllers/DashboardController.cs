using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tenanzia.API.DTOs.Customers;
using Tenanzia.API.DTOs.Dashboard;
using Tenanzia.API.Enums;
using Tenanzia.API.Interfaces;
using Tenanzia.API.Models;

namespace Tenanzia.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly TenanziaContext _context;
        private readonly ITenantService _tenantService;

        public DashboardController(TenanziaContext context, ITenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        [HttpGet]
        public IActionResult GetDashboard()
        {
            var tenantId = _tenantService.GetTenantId();

            // Customers
            var customers = _context.Customers
                .Where(c => c.TenantId == tenantId)
                .ToList();

            // Orders
            var orders = _context.Orders
                .Where(o => o.TenantId == tenantId)
                .Include(o => o.Customer)
                .ToList();

            // Tasks
            var tasks = _context.Tasks
                .Where(t => t.TenantId == tenantId)
                .ToList();

            var tenant = _context.Tenants.FirstOrDefault(t => t.Id == tenantId);



            var topCustomers = _context.Orders
    .Where(o => o.TenantId == tenantId && o.Status == "Completed")
    .GroupBy(o => o.CustomerId)
    .Select(g => new
    {
        CustomerId = g.Key,
        CustomerName = g.First().Customer.Name,
        TotalSpent = g.Sum(o => o.TotalAmount),
        TotalOrders = g.Count()
    })
    .OrderByDescending(c => c.TotalSpent)
    .Take(5)
    .ToList();

            var dashboard = new DashboardResponseDto
            {
                CompanyName = tenant?.Name ?? "", // ← جديد

                // Customers
                TotalCustomers = customers.Count,
                ActiveCustomers = customers.Count(c => c.Status == "Active"),

                // Orders
                TotalOrders = orders.Count,
                PendingOrders = orders.Count(o => o.Status == OrderStatus.Pending),
                CompletedOrders = orders.Count(o => o.Status == OrderStatus.Completed),
                TotalRevenue = orders
                    .Where(o => o.Status == OrderStatus.Completed)
                    .Sum(o => o.TotalAmount),

                // Tasks
                TotalTasks = tasks.Count,
                ToDoTasks = tasks.Count(t => t.Status == TaskStatusEnum.ToDo),
                InProgressTasks = tasks.Count(t => t.Status == TaskStatusEnum.InProgress),
                CompletedTasks = tasks.Count(t => t.Status == TaskStatusEnum.Completed),

                // Recent 5
                RecentOrders = orders
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(5)
                    .Select(o => new RecentOrderDto
                    {
                        Id = o.Id,
                        CustomerName = o.Customer.Name,
                        TotalAmount = o.TotalAmount,
                        Status = o.Status,
                        CreatedAt = o.CreatedAt
                    }).ToList(),

                RecentCustomers = customers
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(5)
                    .Select(c => new RecentCustomerDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Email = c.Email,
                        Status = c.Status,
                        CreatedAt = c.CreatedAt
                    }).ToList(),
                TopCustomers = topCustomers.Select(c => new TopCustomerDto
                {
                    CustomerId = c.CustomerId,
                    CustomerName = c.CustomerName,
                    TotalSpent = c.TotalSpent,
                    TotalOrders = c.TotalOrders
                }).ToList()
            };

            return Ok(dashboard);
        }

        [HttpGet("revenue-chart")]
        public IActionResult GetRevenueChart()
        {
            var tenantId = _tenantService.GetTenantId();

            var last6Months = Enumerable.Range(0, 6)
                .Select(i => DateTime.UtcNow.AddMonths(-i))
                .Select(d => new { Year = d.Year, Month = d.Month, Label = d.ToString("MMM yyyy") })
                .Reverse()
                .ToList();

            var completedOrders = _context.Orders
                .Where(o => o.TenantId == tenantId &&
                            o.Status == "Completed" &&
                            o.CreatedAt >= DateTime.UtcNow.AddMonths(-6))
                .ToList();

            var chartData = last6Months.Select(m => new
            {
                Label = m.Label,
                Revenue = completedOrders
                    .Where(o => o.CreatedAt.Year == m.Year && o.CreatedAt.Month == m.Month)
                    .Sum(o => o.TotalAmount),
                Orders = completedOrders
                    .Count(o => o.CreatedAt.Year == m.Year && o.CreatedAt.Month == m.Month)
            }).ToList();

            return Ok(chartData);
        }
    }
}
