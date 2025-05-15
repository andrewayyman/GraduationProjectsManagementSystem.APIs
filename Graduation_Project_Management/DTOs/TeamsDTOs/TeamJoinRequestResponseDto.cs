namespace Graduation_Project_Management.DTOs.TeamsDTOs
{
    public class TeamJoinRequestResponseDto
    {
        public int RequestId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string? Message { get; set; }
        public string Status { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
    }


}
