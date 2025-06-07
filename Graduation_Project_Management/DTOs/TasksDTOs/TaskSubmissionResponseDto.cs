namespace Graduation_Project_Management.DTOs.TasksDTOs
{
    public class TaskSubmissionResponseDto
    {
        public int SubmissionId { get; set; }
        public int TaskId { get; set; }
        public string FileUrl { get; set; }
        public string Comments { get; set; }
        public string SubmittedAt { get; set; }
    }
}
