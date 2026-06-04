namespace Tenanzia.API.Models
{
    public class UserTenant
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TenantId { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public User User { get; set; } = null!;
        public Tenant Tenant { get; set; } = null!;
    }
}
