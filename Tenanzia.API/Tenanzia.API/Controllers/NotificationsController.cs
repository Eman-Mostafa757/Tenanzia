using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tenanzia.API.Interfaces;
using Tenanzia.API.Models;

namespace Tenanzia.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly TenanziaContext _context;
        private readonly ITenantService _tenantService;

        public NotificationsController(TenanziaContext context, ITenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        // GET: api/notifications
        [HttpGet]
        public IActionResult GetAll()
        {
            var tenantId = _tenantService.GetTenantId();
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var notifications = _context.Notifications
                .Where(n => n.TenantId == tenantId && n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(20)
                .Select(n => new
                {
                    n.Id,
                    n.Title,
                    n.Message,
                    n.Type,
                    n.IsRead,
                    n.CreatedAt
                }).ToList();

            return Ok(notifications);
        }

        // GET: api/notifications/unread-count
        [HttpGet("unread-count")]
        public IActionResult GetUnreadCount()
        {
            var tenantId = _tenantService.GetTenantId();
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var count = _context.Notifications
                .Count(n => n.TenantId == tenantId && n.UserId == userId && !n.IsRead);

            return Ok(new { count });
        }

        // PATCH: api/notifications/{id}/read
        [HttpPatch("{id}/read")]
        public IActionResult MarkAsRead(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var notification = _context.Notifications
                .FirstOrDefault(n => n.Id == id && n.UserId == userId);

            if (notification == null)
                return NotFound();

            notification.IsRead = true;
            _context.SaveChanges();

            return Ok();
        }

        // PATCH: api/notifications/read-all
        [HttpPatch("read-all")]
        public IActionResult MarkAllAsRead()
        {
            var tenantId = _tenantService.GetTenantId();
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var unread = _context.Notifications
                .Where(n => n.TenantId == tenantId && n.UserId == userId && !n.IsRead)
                .ToList();

            foreach (var n in unread)
                n.IsRead = true;

            _context.SaveChanges();
            return Ok();
        }

    }
}
