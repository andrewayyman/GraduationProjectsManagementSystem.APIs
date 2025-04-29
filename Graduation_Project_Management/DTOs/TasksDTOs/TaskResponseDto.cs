using Domain.Enums;

namespace Graduation_Project_Management.DTOs.TasksDTOs
{
    public class TaskResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? Deadline { get; set; }
        public string Status { get; set; }
        public int TeamId { get; set; }
        public int SupervisorId { get; set; }
        public int? AssignedStudentId { get; set; }
        public int? ProjectIdeaId { get; set; }
        public string Message { get; set; }
    }
}