namespace Tenanzia.API.Models
{
    public class TaskComment
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public TaskItem Task { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
