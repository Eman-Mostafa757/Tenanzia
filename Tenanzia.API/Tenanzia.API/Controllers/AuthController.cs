using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tenanzia.API.DTOs;
using Tenanzia.API.Helpers;
using Tenanzia.API.Interfaces;
using Tenanzia.API.Models;
using Tenanzia.API.Services;

namespace Tenanzia.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly TenanziaContext _context;
        private readonly JwtHelper _jwtHelper;
        private readonly ITenantService _tenantService;

        public AuthController(TenanziaContext context ,JwtHelper jwtHelper, ITenantService tenantService)
        {
            _context = context;
            _jwtHelper = jwtHelper;
            _tenantService = tenantService;
        }
        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDto dto,
    [FromServices] EmailService emailService)
        {
            //// امسح الأكونتات القديمة اللي مش verified
            //var expiredAccounts = _context.Users
            //    .Where(u => !u.IsEmailVerified &&
            //                u.EmailVerificationExpiry < DateTime.UtcNow)
            //    .ToList();
            //_context.Users.RemoveRange(expiredAccounts);
            //_context.SaveChanges();

            if (_context.Users.Any(u => u.Email == dto.Email))
                return BadRequest("Email already exists");

            var verificationToken = Guid.NewGuid().ToString();

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = PasswordHelper.HashPassword(dto.Password),
                IsEmailVerified = false,
                EmailVerificationToken = verificationToken,
                EmailVerificationExpiry = DateTime.UtcNow.AddHours(24)
            };

            _context.Users.Add(user);

            // Create Tenant
            var tenant = new Tenant { Name = dto.CompanyName, CreatedAt = DateTime.UtcNow };
            _context.Tenants.Add(tenant);
            _context.SaveChanges();

            // Assign Owner Role
            var ownerRole = _context.Roles.FirstOrDefault(r => r.Name == "Owner");
            if (ownerRole != null)
            {
                _context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = ownerRole.Id });
                _context.UserTenants.Add(new UserTenant { UserId = user.Id, TenantId = tenant.Id });
            }

            // Free Plan
            var freePlan = _context.Plans.FirstOrDefault(p => p.Name == "Free");
            if (freePlan != null)
            {
                _context.Subscriptions.Add(new Models.Subscription
                {
                    TenantId = tenant.Id,
                    PlanId = freePlan.Id,
                    Status = "Active",
                    StartDate = DateTime.UtcNow
                });
            }

            _context.SaveChanges();

            // Send Verification Email
            var baseUrl = "https://tenanzia.vercel.app";
            await emailService.SendVerificationEmail(user.Email, user.Username, verificationToken, baseUrl);

            return Ok("Registration successful! Please check your email to verify your account.");
        }


        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login(LoginDto dto)
        {
            // ابحثي بالـ Email بس الأول
            var user = _context.Users
                .FirstOrDefault(x => x.Email == dto.Email);

            if (user == null)
                return Unauthorized("Invalid credentials");

            // بعدين تحققي من الـ Password بـ BCrypt.Verify
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid credentials");
            // Check Email Verification
            if (!user.IsEmailVerified)
                return Unauthorized("Please verify your email before logging in");


            var userTenant = _context.UserTenants
                .Include(ut => ut.Tenant)
                .FirstOrDefault(x => x.UserId == user.Id);

            if (userTenant == null)
                return Unauthorized("User has no tenant");

            var userRole = _context.UserRoles
                .Include(ur => ur.Role)
                .FirstOrDefault(ur => ur.UserId == user.Id);

            var roleName = userRole?.Role?.Name ?? "Employee";

            var token = _jwtHelper.GenerateToken(user, userTenant.TenantId, userTenant.Tenant.Name, roleName);
            return Ok(new { token });
        }

        // Verify Email
        [HttpGet("verify-email")]
        public IActionResult VerifyEmail([FromQuery] string token)
        {
            var user = _context.Users.FirstOrDefault(u =>
                u.EmailVerificationToken == token &&
                u.EmailVerificationExpiry > DateTime.UtcNow);

            if (user == null)
                return BadRequest("Invalid or expired verification link");

            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationExpiry = null;
            _context.SaveChanges();

            return Ok("Email verified successfully! You can now login.");
        }

        // Forgot Password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] string email,
            [FromServices] EmailService emailService)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
                return Ok("If this email exists, you will receive a reset link.");

            var resetToken = Guid.NewGuid().ToString();
            user.PasswordResetToken = resetToken;
            user.PasswordResetExpiry = DateTime.UtcNow.AddHours(1);
            _context.SaveChanges();

            var baseUrl = "https://tenanzia.vercel.app";
            await emailService.SendPasswordResetEmail(user.Email, user.Username, resetToken, baseUrl);

            return Ok("Password reset link sent to your email.");
        }

        // Reset Password
        [HttpPost("reset-password")]
        public IActionResult ResetPassword(ResetPasswordDto dto)
        {
            var user = _context.Users.FirstOrDefault(u =>
                u.PasswordResetToken == dto.Token &&
                u.PasswordResetExpiry > DateTime.UtcNow);

            if (user == null)
                return BadRequest("Invalid or expired reset link");

            user.PasswordHash = PasswordHelper.HashPassword(dto.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetExpiry = null;
            _context.SaveChanges();

            return Ok("Password reset successfully!");
        }

        [Authorize]
        [HttpGet("users")]
        public IActionResult GetTenantUsers()
        {
            var tenantId = _tenantService.GetTenantId();

            var users = _context.UserTenants
                .Where(ut => ut.TenantId == tenantId)
                .Include(ut => ut.User)
                .ThenInclude(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Select(ut => new
                {
                    ut.User.Id,
                    ut.User.Username,
                    ut.User.Email,
                    Role = ut.User.UserRoles
                        .Select(ur => ur.Role.Name)
                        .FirstOrDefault() ?? "Employee"
                }).ToList();

            return Ok(users);
        }

        [Authorize]
        [HttpPost("invite")]
        public IActionResult InviteEmployee(InviteDto dto)
        {
            var tenantId = _tenantService.GetTenantId();

            // تأكد إن الـ user الحالي Owner
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var currentUserRole = _context.UserRoles
                .Include(ur => ur.Role)
                .FirstOrDefault(ur => ur.UserId == currentUserId);

            if (currentUserRole?.Role?.Name != "Owner")
                return Forbid();

            // تأكد إن الـ Email مش موجود
            if (_context.Users.Any(x => x.Email == dto.Email))
                return BadRequest("Email already in use");

            // 1. Create user
            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = PasswordHelper.HashPassword(dto.Password)
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            // 2. Assign Role
            var role = _context.Roles.FirstOrDefault(r => r.Name == dto.Role);
            if (role != null)
            {
                _context.UserRoles.Add(new UserRole
                {
                    UserId = user.Id,
                    RoleId = role.Id
                });
            }

            // 3. Link to same Tenant
            _context.UserTenants.Add(new UserTenant
            {
                UserId = user.Id,
                TenantId = tenantId
            });

            _context.SaveChanges();

            return Ok("Employee invited successfully");
        }

        // تغيير الـ Role
        [Authorize]
        [HttpPut("users/{userId}/role")]
        public IActionResult UpdateUserRole(int userId, [FromBody] string newRole)
        {
            var tenantId = _tenantService.GetTenantId();

            // تأكد إن الـ user ده في نفس الـ tenant
            var userTenant = _context.UserTenants
                .FirstOrDefault(ut => ut.UserId == userId && ut.TenantId == tenantId);

            if (userTenant == null)
                return NotFound("User not found in this tenant");

            // تأكد إن الـ Role موجود
            var role = _context.Roles.FirstOrDefault(r => r.Name == newRole);
            if (role == null)
                return BadRequest("Invalid role");

            // عدّل الـ Role
            var userRole = _context.UserRoles.FirstOrDefault(ur => ur.UserId == userId);
            if (userRole != null)
                userRole.RoleId = role.Id;
            else
                _context.UserRoles.Add(new UserRole { UserId = userId, RoleId = role.Id });

            _context.SaveChanges();
            return Ok("Role updated successfully");
        }

        // حذف الـ User من الـ Tenant
        [Authorize]
        [HttpDelete("users/{userId}")]
        public IActionResult RemoveUser(int userId)
        {
            var tenantId = _tenantService.GetTenantId();

            // منع حذف نفسك
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (userId == currentUserId)
                return BadRequest("Cannot remove yourself");

            var userTenant = _context.UserTenants
                .FirstOrDefault(ut => ut.UserId == userId && ut.TenantId == tenantId);

            if (userTenant == null)
                return NotFound("User not found in this tenant");

            _context.UserTenants.Remove(userTenant);
            _context.SaveChanges();

            return Ok("User removed successfully");
        }

        [Authorize]
        [HttpGet("users/{userId}/profile")]
        public IActionResult GetUserProfile(int userId)
        {
            var tenantId = _tenantService.GetTenantId();

            // تأكد إن الـ User في نفس الـ Tenant
            var userTenant = _context.UserTenants
                .Where(ut => ut.UserId == userId && ut.TenantId == tenantId)
                .Include(ut => ut.User)
                .ThenInclude(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefault();

            if (userTenant == null)
                return NotFound("User not found in this tenant");

            var user = userTenant.User;

            // جيبي كل Tasks بتاعته
            var tasks = _context.Tasks
                .Where(t => t.AssignedToUserId == userId && t.TenantId == tenantId)
                .ToList();

            var now = DateTime.UtcNow;

            var profile = new
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.UserRoles.Select(ur => ur.Role.Name).FirstOrDefault() ?? "Employee",
                JoinedAt = userTenant.JoinedAt,

                // Stats
                TotalTasks = tasks.Count,
                CompletedTasks = tasks.Count(t => t.Status == "Completed"),
                InProgressTasks = tasks.Count(t => t.Status == "InProgress"),
                ToDoTasks = tasks.Count(t => t.Status == "ToDo"),
                CancelledTasks = tasks.Count(t => t.Status == "Cancelled"),
                OverdueTasks = tasks.Count(t => t.DueDate < now && t.Status != "Completed" && t.Status != "Cancelled"),
                CompletionRate = tasks.Count == 0 ? 0 :
                    (int)((double)tasks.Count(t => t.Status == "Completed") / tasks.Count * 100),

                // Activity Status
                ActivityStatus = tasks.Any(t => t.Status == "InProgress") ? "Active" :
                                tasks.Any(t => t.DueDate < now && t.Status != "Completed" && t.Status != "Cancelled") ? "Overdue" :
                                "Idle",

                // Tasks List
                Tasks = tasks.OrderByDescending(t => t.CreatedAt).Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    t.Status,
                    t.Priority,
                    t.DueDate,
                    t.CreatedAt,
                    IsOverdue = t.DueDate < now && t.Status != "Completed" && t.Status != "Cancelled"
                }).ToList()
            };

            return Ok(profile);
        }

        [Authorize]
        [HttpGet("users/workload")]
        public IActionResult GetTeamWorkload()
        {
            var tenantId = _tenantService.GetTenantId();
            var now = DateTime.UtcNow;

            var users = _context.UserTenants
                .Where(ut => ut.TenantId == tenantId)
                .Include(ut => ut.User)
                .ThenInclude(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ToList();

            var workload = users.Select(ut =>
            {
                var tasks = _context.Tasks
                    .Where(t => t.AssignedToUserId == ut.UserId && t.TenantId == tenantId)
                    .ToList();

                var activeTasks = tasks.Count(t => t.Status == "ToDo" || t.Status == "InProgress");
                var overdueTasks = tasks.Count(t => t.DueDate < now && t.Status != "Completed" && t.Status != "Cancelled");

                return new
                {
                    Id = ut.User.Id,
                    Username = ut.User.Username,
                    Email = ut.User.Email,
                    Role = ut.User.UserRoles.Select(ur => ur.Role.Name).FirstOrDefault() ?? "Employee",
                    TotalTasks = tasks.Count,
                    ActiveTasks = activeTasks,
                    CompletedTasks = tasks.Count(t => t.Status == "Completed"),
                    OverdueTasks = overdueTasks,
                    CompletionRate = tasks.Count == 0 ? 0 :
                        (int)((double)tasks.Count(t => t.Status == "Completed") / tasks.Count * 100),
                    ActivityStatus = tasks.Any(t => t.Status == "InProgress") ? "Active" :
                                    overdueTasks > 0 ? "Overdue" : "Idle",
                    WorkloadLevel = activeTasks == 0 ? "Free" :
                                   activeTasks <= 3 ? "Normal" :
                                   activeTasks <= 6 ? "Heavy" : "Overloaded"
                };
            }).ToList();

            return Ok(workload);
        }

        [Authorize]
        [HttpPost("users/{userId}/assign-task/{taskId}")]
        public IActionResult AssignTask(int userId, int taskId)
        {
            var tenantId = _tenantService.GetTenantId();

            // تأكد إن الـ User في نفس الـ Tenant
            var userTenant = _context.UserTenants
                .FirstOrDefault(ut => ut.UserId == userId && ut.TenantId == tenantId);
            if (userTenant == null)
                return NotFound("User not found in this tenant");

            // تأكد إن الـ Task في نفس الـ Tenant
            var task = _context.Tasks
                .FirstOrDefault(t => t.Id == taskId && t.TenantId == tenantId);
            if (task == null)
                return NotFound("Task not found");

            task.AssignedToUserId = userId;
            _context.SaveChanges();

            return Ok("Task assigned successfully");
        }


        [Authorize]
        [HttpPut("users/{userId}")]
        public IActionResult UpdateUser(int userId, UpdateUserDto dto)
        {
            var tenantId = _tenantService.GetTenantId();

            // تأكد إن اليوزر في نفس الـ Tenant
            var userTenant = _context.UserTenants
                .Include(ut => ut.User)
                .FirstOrDefault(ut => ut.UserId == userId && ut.TenantId == tenantId);

            if (userTenant == null)
                return NotFound("User not found");

            // تأكد إن الإيميل مش مستخدم من يوزر تاني
            var emailExists = _context.Users
                .Any(u => u.Email == dto.Email && u.Id != userId);

            if (emailExists)
                return BadRequest("Email already in use");

            // تعديل البيانات
            userTenant.User.Username = dto.Username;
            userTenant.User.Email = dto.Email;
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                userTenant.User.PasswordHash = PasswordHelper.HashPassword(dto.Password);
            }
            _context.SaveChanges();

            return Ok("User updated successfully");
        }

        // GET: api/Auth/me
        [Authorize]
        [HttpGet("me")]
        public IActionResult GetMe()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var tenantId = _tenantService.GetTenantId();

            var user = _context.Users
                .FirstOrDefault(u => u.Id == userId);

            var tenant = _context.Tenants
                .FirstOrDefault(t => t.Id == tenantId);

            var role = _context.UserRoles
                .Include(ur => ur.Role)
                .FirstOrDefault(ur => ur.UserId == userId)?.Role?.Name ?? "Employee";

            if (user == null) return NotFound();

            return Ok(new
            {
                user.Id,
                user.Username,
                user.Email,
                Role = role,
                Company = new
                {
                    tenant?.Id,
                    tenant?.Name,
                    tenant?.CreatedAt
                }
            });
        }

        // PUT: api/Auth/me
        [Authorize]
        [HttpPut("me")]
        public IActionResult UpdateMe(UpdateUserDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return NotFound();

            if (!string.IsNullOrEmpty(dto.Username))
                user.Username = dto.Username;

            if (!string.IsNullOrEmpty(dto.Email))
            {
                if (_context.Users.Any(u => u.Email == dto.Email && u.Id != userId))
                    return BadRequest("Email already in use");
                user.Email = dto.Email;
            }

            if (!string.IsNullOrEmpty(dto.Password))
                user.PasswordHash = PasswordHelper.HashPassword(dto.Password);

            _context.SaveChanges();
            return Ok("Profile updated successfully");
        }

        // PUT: api/Auth/company
        [Authorize]
        [HttpPut("company")]
        public IActionResult UpdateCompany([FromBody] string newName)
        {
            var tenantId = _tenantService.GetTenantId();
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // بس الـ Owner يقدر يغير اسم الشركة
            var role = _context.UserRoles
                .Include(ur => ur.Role)
                .FirstOrDefault(ur => ur.UserId == userId)?.Role?.Name;

            if (role != "Owner")
                return Forbid();

            var tenant = _context.Tenants.FirstOrDefault(t => t.Id == tenantId);
            if (tenant == null) return NotFound();

            tenant.Name = newName;
            _context.SaveChanges();

            return Ok("Company name updated successfully");
        }

    }
}
