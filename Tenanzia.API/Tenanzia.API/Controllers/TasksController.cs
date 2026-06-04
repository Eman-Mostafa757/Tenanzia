using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Tenanzia.API.DTOs.Tasks;
using Tenanzia.API.Enums;
using Tenanzia.API.Interfaces;
using Tenanzia.API.Models;
using Tenanzia.API.Services;

namespace Tenanzia.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly TenanziaContext _context;
        private readonly ITenantService _tenantService;
        private readonly NotificationService _notificationService;
        private readonly SubscriptionLimitService _subscriptionService;

        public TasksController(TenanziaContext context, ITenantService tenantService, NotificationService notificationService, SubscriptionLimitService subscriptionService)
        {
            _context = context;
            _tenantService = tenantService;
            _notificationService = notificationService;
            _subscriptionService = subscriptionService;
        }

        // GET: api/tasks
        [HttpGet("GetAll")]
        public IActionResult GetAll([FromQuery] string? status, [FromQuery] string? priority)
        {
            var tenantId = _tenantService.GetTenantId();
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var isManager = User.IsInRole("Manager") || User.IsInRole("Owner");

            var query = _context.Tasks
        .Where(t => t.TenantId == tenantId)
        .AsQueryable();

            // لو مش Manager/Owner → يشوف Tasks بتاعته بس
            if (!isManager)
                query = query.Where(t => t.AssignedToUserId == userId);

            var filtered = query.AsQueryable();

            if (!string.IsNullOrEmpty(status))
                filtered = filtered.Where(t => t.Status == status);

            if (!string.IsNullOrEmpty(priority))
                filtered = filtered.Where(t => t.Priority == priority);

            var result = query
         .Include(t => t.AssignedToUser)
         .Select(t => new TaskResponseDto
         {
             Id = t.Id,
             Title = t.Title,
             Description = t.Description,
             Status = t.Status,
             Priority = t.Priority,
             DueDate = t.DueDate,
             AssignedToUserId = t.AssignedToUserId,
             AssignedToUsername = t.AssignedToUser != null ? t.AssignedToUser.Username : null,
             CreatedAt = t.CreatedAt
         }).ToList();

            return Ok(result);
        }

        // GET: api/tasks/kanban
        [HttpGet("kanban")]
        public IActionResult GetKanban()
        {
            var tenantId = _tenantService.GetTenantId();
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var isManager = User.IsInRole("Manager") || User.IsInRole("Owner");

            var query = _context.Tasks
       .Where(t => t.TenantId == tenantId)
       .AsQueryable();

            // لو مش Manager/Owner → يشوف Tasks بتاعته بس
            if (!isManager)
                query = query.Where(t =>
                    t.AssignedToUserId == userId ||
                    t.AssignedToUserId == null);

            //    var tasks = query
            //.Include(t => t.AssignedToUser)
            //.ToList();
            var tasks = query.Include(t => t.AssignedToUser)
                         .Include(t => t.Comments)
                         .ToList();

            // جيبي الـ LastReadAt لكل task للـ user ده
            var reads = _context.TaskCommentReads
                .Where(r => r.UserId == userId)
                .ToDictionary(r => r.TaskId, r => r.LastReadAt);
            TaskResponseDto MapTask(TaskItem t) => new TaskResponseDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                Priority = t.Priority,
                DueDate = t.DueDate,
                AssignedToUserId = t.AssignedToUserId,
                AssignedToUsername = t.AssignedToUser?.Username,
                CreatedAt = t.CreatedAt,
                // هل فيه comments جديدة من غير الـ user ده؟
                HasUnread = t.Comments.Any(c =>
                    c.UserId != userId &&
                    (!reads.ContainsKey(t.Id) || c.CreatedAt > reads[t.Id]))
            };

            var kanban = new
            {
                ToDo = tasks
         .Where(t => t.Status == TaskStatusEnum.ToDo)
         .Select(t => MapTask(t)).ToList(),
                InProgress = tasks
         .Where(t => t.Status == TaskStatusEnum.InProgress)
         .Select(t => MapTask(t)).ToList(),
                Completed = tasks
         .Where(t => t.Status == TaskStatusEnum.Completed)
         .Select(t => MapTask(t)).ToList(),
                Cancelled = tasks
         .Where(t => t.Status == TaskStatusEnum.Cancelled)
         .Select(t => MapTask(t)).ToList()
            };

            return Ok(kanban);
        }

        // GET: api/tasks/5
        [HttpGet("GetById/{id}")]
        public IActionResult GetById(int id)
        {
            var tenantId = _tenantService.GetTenantId();

            var task = _context.Tasks
                .Include(t => t.AssignedToUser)
                .Where(t => t.TenantId == tenantId && t.Id == id)
                .Select(t => new TaskResponseDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status,
                    Priority = t.Priority,
                    DueDate = t.DueDate,
                    AssignedToUserId = t.AssignedToUserId,
                    AssignedToUsername = t.AssignedToUser != null ? t.AssignedToUser.Username : null,
                    CreatedAt = t.CreatedAt
                }).FirstOrDefault();

            if (task == null)
                return NotFound("Task not found");

            return Ok(task);
        }

        // POST: api/tasks
        [HttpPost("Create")]
        public IActionResult Create(CreateTaskDto dto)
        {
            var tenantId = _tenantService.GetTenantId();
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var username = User.FindFirst(ClaimTypes.Name)!.Value;
            var isManager = User.IsInRole("Manager") || User.IsInRole("Owner");
            var (canAdd, message) = _subscriptionService.CanAddTask(tenantId);
            if (!canAdd)
                return BadRequest(new { error = message, upgradeRequired = true });

            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                Priority = dto.Priority,
                DueDate = dto.DueDate,
                Status = TaskStatusEnum.ToDo,
                TenantId = tenantId,
                CreatedAt = DateTime.UtcNow,

                // لو Manager → يحدد هو، لو Employee → تتعين له تلقائياً
                AssignedToUserId = isManager ? dto.AssignedToUserId : userId
            };

            _context.Tasks.Add(task);
            _context.SaveChanges();

            // Activity Log
            _context.TaskActivities.Add(new TaskActivity
            {
                TaskId = task.Id,
                UserId = userId,
                Action = $"Task created by {username}",
                CreatedAt = DateTime.UtcNow
            });

            if (task.AssignedToUserId.HasValue)
            {
                var assignedUser = _context.Users.FirstOrDefault(u => u.Id == task.AssignedToUserId);
                if (!isManager)
                {
                    _context.TaskActivities.Add(new TaskActivity
                    {
                        TaskId = task.Id,
                        UserId = userId,
                        Action = $"Self-assigned by {username}",
                        CreatedAt = DateTime.UtcNow
                    });
                }
                else
                {
                    _context.TaskActivities.Add(new TaskActivity
                    {
                        TaskId = task.Id,
                        UserId = userId,
                        Action = $"Assigned to {assignedUser?.Username ?? "someone"} by {username}",
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
         

            _context.SaveChanges();

            if (task.AssignedToUserId.HasValue && task.AssignedToUserId != userId)
            {
                _notificationService.Create(
                    tenantId,
                    task.AssignedToUserId.Value,
                    "New Task Assigned",
                    $"You have been assigned to: {task.Title}",
                    "task"
                );
            }
            return CreatedAtAction(nameof(GetById), new { id = task.Id }, task.Id);
        }

        // PUT: api/tasks/5
        [HttpPut("Update/{id}")]
        public IActionResult Update(int id, UpdateTaskDto dto)
        {
            var tenantId = _tenantService.GetTenantId();
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var task = _context.Tasks
                .FirstOrDefault(t => t.TenantId == tenantId && t.Id == id);

            if (task == null)
                return NotFound("Task not found");

            if (dto.Title != null) task.Title = dto.Title;
            if (dto.Description != null) task.Description = dto.Description;
            if (dto.Status != null) task.Status = dto.Status;
            if (dto.Priority != null) task.Priority = dto.Priority;
            if (dto.DueDate.HasValue) task.DueDate = dto.DueDate;
            if (dto.AssignedToUserId.HasValue) task.AssignedToUserId = dto.AssignedToUserId;

            _context.SaveChanges();
            if (task.AssignedToUserId.HasValue && task.AssignedToUserId != userId)
            {
                _notificationService.Create(
                    tenantId,
                    task.AssignedToUserId.Value,
                    "New Task Assigned",
                    $"You have been assigned to: {task.Title}",
                    "task"
                );
            }

            return Ok("Task updated successfully");
        }

        // PATCH: api/tasks/5/status
        [HttpPatch("{id}/status")]
        public IActionResult UpdateStatus(int id, [FromBody] string newStatus)
        {
            var tenantId = _tenantService.GetTenantId();
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var username = User.FindFirst(ClaimTypes.Name)!.Value;

            var validStatuses = new[]
            {
            TaskStatusEnum.ToDo,
            TaskStatusEnum.InProgress,
            TaskStatusEnum.Completed,
            TaskStatusEnum.Cancelled
        };

            if (!validStatuses.Contains(newStatus))
                return BadRequest("Invalid status value");

            var task = _context.Tasks
                .FirstOrDefault(t => t.TenantId == tenantId && t.Id == id);

            if (task == null)
                return NotFound("Task not found");

            var oldStatus = task.Status;
            task.Status = newStatus;

            // Activity Log
            _context.TaskActivities.Add(new TaskActivity
            {
                TaskId = id,
                UserId = userId,
                Action = $"Status changed from {oldStatus} to {newStatus} by {username}",
                CreatedAt = DateTime.UtcNow
            });
            _context.SaveChanges();

            return Ok("Status updated successfully");
        }

        // DELETE: api/tasks/5
        [HttpDelete("Delete/{id}")]
        public IActionResult Delete(int id)
        {
            var tenantId = _tenantService.GetTenantId();

            var task = _context.Tasks
                .FirstOrDefault(t => t.TenantId == tenantId && t.Id == id);

            if (task == null)
                return NotFound("Task not found");

            _context.Tasks.Remove(task);
            _context.SaveChanges();

            return Ok("Task deleted successfully");
        }

        // GET: api/tasks/5/details
        [HttpGet("{id}/details")]
        public IActionResult GetTaskDetails(int id)
        {
            var tenantId = _tenantService.GetTenantId();
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var task = _context.Tasks
                .Where(t => t.TenantId == tenantId && t.Id == id)
                .Include(t => t.AssignedToUser)
                .Include(t => t.Comments).ThenInclude(c => c.User)
                .Include(t => t.Activities).ThenInclude(a => a.User)
                .FirstOrDefault();

            if (task == null)
                return NotFound("Task not found");

            // Mark as Read — لما حد يفتح الـ details
            var read = _context.TaskCommentReads
                .FirstOrDefault(r => r.TaskId == id && r.UserId == userId);

            if (read == null)
            {
                _context.TaskCommentReads.Add(new TaskCommentRead
                {
                    TaskId = id,
                    UserId = userId,
                    LastReadAt = DateTime.UtcNow
                });
            }
            else
            {
                read.LastReadAt = DateTime.UtcNow;
            }

            _context.SaveChanges();

            return Ok(new
            {
                task.Id,
                task.Title,
                task.Description,
                task.Status,
                task.Priority,
                task.DueDate,
                task.CreatedAt,
                AssignedToUserId = task.AssignedToUserId,
                AssignedToUsername = task.AssignedToUser?.Username,
                Comments = task.Comments.OrderBy(c => c.CreatedAt).Select(c => new
                {
                    c.Id,
                    c.Content,
                    c.CreatedAt,
                    c.UserId,
                    Username = c.User.Username
                }).ToList(),
                Activities = task.Activities.OrderByDescending(a => a.CreatedAt).Select(a => new
                {
                    a.Id,
                    a.Action,
                    a.CreatedAt,
                    Username = a.User.Username
                }).ToList()
            });
        }

        // POST: api/tasks/5/comments
        [HttpPost("{id}/comments")]
        public IActionResult AddComment(int id, [FromBody] string content)
        {
            var tenantId = _tenantService.GetTenantId();
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var username = User.FindFirst(ClaimTypes.Name)!.Value;

            var task = _context.Tasks
                .FirstOrDefault(t => t.TenantId == tenantId && t.Id == id);

            if (task == null)
                return NotFound("Task not found");

            // أضيف الـ Comment
            var comment = new TaskComment
            {
                TaskId = id,
                UserId = userId,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };
            _context.TaskComments.Add(comment);
            // في AddComment — notify الـ assigned user
            if (task.AssignedToUserId.HasValue && task.AssignedToUserId != userId)
            {
                _notificationService.Create(
                    tenantId,
                    task.AssignedToUserId.Value,
                    "New Comment",
                    $"{username} commented on: {task.Title}",
                    "comment"
                );
            }

            // سجّل في الـ Activity
            _context.TaskActivities.Add(new TaskActivity
            {
                TaskId = id,
                UserId = userId,
                Action = $"Comment added by {username}",
                CreatedAt = DateTime.UtcNow
            });

            _context.SaveChanges();
            return Ok("Comment added");
        }

        // DELETE: api/tasks/5/comments/3
        [HttpDelete("{id}/comments/{commentId}")]
        public IActionResult DeleteComment(int id, int commentId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var comment = _context.TaskComments
                .FirstOrDefault(c => c.Id == commentId && c.TaskId == id);

            if (comment == null)
                return NotFound("Comment not found");

            // بس صاحب الـ comment يحذفه
            if (comment.UserId != userId)
                return Forbid();

            _context.TaskComments.Remove(comment);
            _context.SaveChanges();
            return Ok("Comment deleted");
        }
    }
}
