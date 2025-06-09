namespace Graduation_Project_Management.DTOs.MeetingDto
{
    public class UpdateMeetingDto
    {
        public string? Title { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public string? MeetingLink { get; set; }
        public string? Objectives { get; set; }
        public string? Comment { get; set; }
    }
}
