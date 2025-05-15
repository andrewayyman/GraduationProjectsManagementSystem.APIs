using Domain.Entities;
using Graduation_Project_Management.DTOs.SupervisorDTOs;

namespace Graduation_Project_Management.DTOs.ProjectIdeasDTOs
{
    public class ProjectIdeaDto
    {
        public int ProjectIdeaId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<string> TechStack { get; set; }
        public string CreatedAt { get; set; }
        public string Status { get; set; }
        public ShortSupervisorDto Supervisor { get; set; }

    }
}
