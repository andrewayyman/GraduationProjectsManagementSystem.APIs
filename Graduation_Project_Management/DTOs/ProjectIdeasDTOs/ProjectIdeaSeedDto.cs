namespace Graduation_Project_Management.DTOs.ProjectIdeasDTOs
{
    public class ProjectIdeaSeedDto
    {
        public int ProjectIdeaId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<string> TechStack { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdaterdAt { get; set; }
        public string Status { get; set; }
        public int TeamId { get; set; }
        public int SupervisorId { get; set; }
        public bool IsCompleted { get; set; }   
        public DateTime? CompletedAt { get; set; }

        public string SupervisorEmail { get; set; }
    }
}
