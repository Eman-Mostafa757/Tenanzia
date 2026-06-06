using Tenanzia.API.DTOs.Customers;
using Tenanzia.API.DTOs.Products;

namespace Tenanzia.API.DTOs.Dashboard
{
    public class DashboardResponseDto
    {
        public string CompanyName { get; set; } = string.Empty;

        // Customers
        public int TotalCustomers { get; set; }
        public int ActiveCustomers { get; set; }

        // Orders
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public decimal TotalRevenue { get; set; }

        // Tasks
        public int TotalTasks { get; set; }
        public int ToDoTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int CompletedTasks { get; set; }

        // Recent
        public List<RecentOrderDto> RecentOrders { get; set; } = new();
        public List<RecentCustomerDto> RecentCustomers { get; set; } = new();
        public List<TopCustomerDto> TopCustomers { get; set; } = new();
        public List<LowStockProductDto> LowStockProducts { get; set; } = new();


    }
}
