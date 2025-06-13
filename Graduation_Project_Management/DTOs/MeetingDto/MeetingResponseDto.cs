namespace Graduation_Project_Management.DTOs.MeetingDto
{
    public class MeetingResponseDto
    {
        public int MeetingId { get; set; }
        public string Title { get; set; }
        public DateTime ScheduledAt { get; set; }
        public string? MeetingLink { get; set; }
        public string? Objectives { get; set; }
        public string? Comment { get; set; }
        public string? TeamName { get; set; }
        public int? TeamId { get; set; }
        public string? SupervisorName { get; set; }
        public string Message { get; set; }
    }
}
