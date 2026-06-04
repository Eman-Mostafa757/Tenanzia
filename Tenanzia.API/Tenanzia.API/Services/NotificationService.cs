using Tenanzia.API.Models;

namespace Tenanzia.API.Services
{
    public class NotificationService
    {
        private readonly TenanziaContext _context;

        public NotificationService(TenanziaContext context)
        {
            _context = context;
        }

        public void Create(int tenantId, int userId, string title, string message, string type)
        {
            _context.Notifications.Add(new Notification
            {
                TenantId = tenantId,
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
            _context.SaveChanges();
        }
    }
}
