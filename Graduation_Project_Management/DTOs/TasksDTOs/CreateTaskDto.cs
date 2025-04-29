using Domain.Enums;

namespace Graduation_Project_Management.DTOs.TasksDTOs
{
    public class CreateTaskDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Deadline { get; set; } = DateTime.UtcNow.AddDays(7);  // with default value of 7 days from now
        public TaskStatusEnum Status { get; set; } = TaskStatusEnum.Backlog;

        public int? AssignedToId { get; set; }
        public int TeamId { get; set; }
        public int? ProjectIdeaId { get; set; }
    }
}