using Domain.Enums;

namespace Graduation_Project_Management.DTOs.TasksDTOs
{
    public class TaskResponseDto
    {
        public int TaskId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? Deadline { get; set; }
        public string Status { get; set; }

        public string TeamName { get; set; }
        public string SupervisorName { get; set; }
        public string AssignedStudentName { get; set; }
        public string ProjectIdeaTitle { get; set; }

        public string Message { get; set; }
    }
}