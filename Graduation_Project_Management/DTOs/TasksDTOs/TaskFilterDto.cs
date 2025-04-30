namespace Graduation_Project_Management.DTOs.TasksDTOs
{
    public class TaskFilterDto
    {
        public int? TeamId { get; set; }
        public int? SupervisorId { get; set; }
        public int? AssignedStudentId { get; set; }
        public string? Status { get; set; }
        public string? DeadlineFrom { get; set; } // Changed to string
        public string? DeadlineTo { get; set; }   // Changed to string
    }
}