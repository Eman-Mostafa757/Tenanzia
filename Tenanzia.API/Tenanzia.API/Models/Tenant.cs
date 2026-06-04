namespace Tenanzia.API.Models
{
    public class Tenant
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<UserTenant> UserTenants { get; set; } = new List<UserTenant>();

    }
}
