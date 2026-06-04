namespace Tenanzia.API.DTOs.Tasks
{
    public class TaskResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public int? AssignedToUserId { get; set; }
        public string? AssignedToUsername { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool HasUnread { get; set; }

    }
}
