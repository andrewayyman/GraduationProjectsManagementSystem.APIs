namespace Graduation_Project_Management.DTOs
{
    public class PublishProjectIdeaDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public List<string>? TechStack { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;                    // date of the project idea

    }
}
