using System.ComponentModel.DataAnnotations;
using Tenanzia.API.Enums;

namespace Tenanzia.API.DTOs.Tasks
{
    public class CreateTaskDto
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public string Priority { get; set; } = TaskPriority.Medium;

        public DateTime? DueDate { get; set; }

        public int? AssignedToUserId { get; set; }
    }
}
