using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ProjectIdeaRequest
    {
        public int Id { get; set; }

        public int ProjectIdeaId { get; set; }
        public ProjectIdea ProjectIdea { get; set; }

        public int SupervisorId { get; set; }
        public Supervisor Supervisor { get; set; }

        public ProjectIdeaStatus Status { get; set; } = ProjectIdeaStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
