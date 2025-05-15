namespace Graduation_Project_Management.DTOs.ProjectIdeasDTOs
{
    public class PublishProjectIdeaDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> TechStack { get; set; } = new();


    }
}
