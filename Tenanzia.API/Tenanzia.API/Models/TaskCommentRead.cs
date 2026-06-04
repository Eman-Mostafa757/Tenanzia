namespace Tenanzia.API.Models
{
    public class TaskCommentRead
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int UserId { get; set; }
        public DateTime LastReadAt { get; set; } = DateTime.UtcNow;

        public TaskItem Task { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
