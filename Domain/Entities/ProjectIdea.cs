using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ProjectIdea
    {
        public int Id { get; set; }
        public string Title { get; set; }                                             // title of the project idea
        public string Description { get; set; }                                       // description of the project idea

        public List<string> TechStack { get; set; } = new();                          // technologies used in the project

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;                    // date of the project idea

        public DateTime? UpdatedAt { get; set; }
        public ProjectIdeaStatus Status { get; set; } = ProjectIdeaStatus.Pending;

        // Relationships
        public int TeamId { get; set; }

        public Team Team { get; set; }                                                // team that the project idea belongs to

        public ICollection<ProjectIdeaRequest> Requests { get; set; } = new List<ProjectIdeaRequest>();

        public int? SupervisorId { get; set; }

        public Supervisor Supervisor { get; set; }
    }
}