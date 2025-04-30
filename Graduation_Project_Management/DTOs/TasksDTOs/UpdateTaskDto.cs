using Domain.Enums;

namespace Graduation_Project_Management.DTOs.TasksDTOs
{
    public class UpdateTaskDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Deadline { get; set; }
        public int AssignedToId { get; set; }
        public TaskStatusEnum Status { get; set; }
    }
}