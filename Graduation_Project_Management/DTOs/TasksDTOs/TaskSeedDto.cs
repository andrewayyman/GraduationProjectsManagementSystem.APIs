using Domain.Enums;
using Microsoft.VisualBasic;

namespace Graduation_Project_Management.DTOs.TasksDTOs
{
    public class TaskSeedDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Deadline { get; set; }
        public DateTime CreatedAt { get; set; }
        public TaskStatusEnum Status { get; set; }
        public int SupervisorId { get; set; }
        public int TeamId { get; set; }
        public string AssignedToEmail { get; set; }  // NEW

        public int AssignedToId { get; set; } 
    }
}
