namespace Tenanzia.API.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = "ToDo";
        public string Priority { get; set; } = "Medium";
        public DateTime? DueDate { get; set; }
        public int? AssignedToUserId { get; set; }
        public int TenantId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User? AssignedToUser { get; set; }
        public Tenant Tenant { get; set; } = null!;
        public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
        public ICollection<TaskActivity> Activities { get; set; } = new List<TaskActivity>();
    }
}
