namespace Graduation_Project_Management.DTOs.MeetingDto
{
    public class CreateMeetingDto
    {
        public string Title { get; set; }
        public DateTime ScheduledAt { get; set; }
        public string? MeetingLink { get; set; }
        public string? Objectives { get; set; }
        public string? Comment { get; set; }
        public int TeamId { get; set; }
    }
}
