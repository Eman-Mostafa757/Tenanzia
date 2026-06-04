using Microsoft.EntityFrameworkCore;
using Tenanzia.API.DTOs.Subscriptions;
using Tenanzia.API.Models;

namespace Tenanzia.API.Services
{
    public class SubscriptionLimitService
    {
        private readonly TenanziaContext _context;

        public SubscriptionLimitService(TenanziaContext context)
        {
            _context = context;
        }

        public (bool canAdd, string message) CanAddCustomer(int tenantId)
        {
            var plan = GetCurrentPlan(tenantId);
            if (plan == null) return (false, "No active subscription");

            if (plan.MaxCustomers == 999999) return (true, "");

            var currentCount = _context.Customers
                .Count(c => c.TenantId == tenantId);

            if (currentCount >= plan.MaxCustomers)
                return (false, $"You've reached the {plan.Name} Plan limit of {plan.MaxCustomers} customers. Upgrade to Pro for unlimited customers.");

            return (true, "");
        }

        public (bool canAdd, string message) CanAddTask(int tenantId)
        {
            var plan = GetCurrentPlan(tenantId);
            if (plan == null) return (false, "No active subscription");

            if (plan.MaxTasks == 999999) return (true, "");

            var currentCount = _context.Tasks
                .Count(t => t.TenantId == tenantId);

            if (currentCount >= plan.MaxTasks)
                return (false, $"You've reached the {plan.Name} Plan limit of {plan.MaxTasks} tasks. Upgrade to Pro for unlimited tasks.");

            return (true, "");
        }

        private Plan? GetCurrentPlan(int tenantId)
        {
            return _context.Subscriptions
                .Where(s => s.TenantId == tenantId && s.Status == "Active")
                .OrderByDescending(s => s.StartDate)
                .Include(s => s.Plan)
                .FirstOrDefault()?.Plan;
        }

        public PlanLimitsDto GetLimits(int tenantId)
        {
            var plan = GetCurrentPlan(tenantId);
            if (plan == null) return new PlanLimitsDto();

            var customerCount = _context.Customers.Count(c => c.TenantId == tenantId);
            var taskCount = _context.Tasks.Count(t => t.TenantId == tenantId);

            return new PlanLimitsDto
            {
                PlanName = plan.Name,
                MaxCustomers = plan.MaxCustomers,
                CurrentCustomers = customerCount,
                MaxTasks = plan.MaxTasks,
                CurrentTasks = taskCount,
                IsUnlimited = plan.MaxCustomers == 999999
            };
        }
    }
}
