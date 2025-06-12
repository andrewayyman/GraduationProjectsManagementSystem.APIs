namespace Graduation_Project_Management.DTOs.StudentDTOs
{
    public class StudentJoinRequestResponseDto
    {
        public int RequestId { get; set; }
        public int TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string? Message { get; set; }
        public string Status { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
    }
}
