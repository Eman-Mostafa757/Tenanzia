using System.ComponentModel.DataAnnotations;

namespace Tenanzia.API.DTOs.Tasks
{
    public class UpdateTaskDto
    {
        [MaxLength(100)]
        public string? Title { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public string? Status { get; set; }
        public string? Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public int? AssignedToUserId { get; set; }
    }
}
