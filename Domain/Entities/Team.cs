using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; }                                                        // name of the team
        public string? Description { get; set; }                                                // description of the team
        public string? TeamDepartment { get; set; }                                             // department of the team

        public bool IsOpenToJoin { get; set; } = true;                                          // if the team is open to join or not
        public int MaxMembers { get; set; } = 6;                                                // max number of members in the team
        public List<string>? TechStack { get; set; } = new();                                   // techs used in the project

        // RelationShips

        // one team can have many students, and one student can be in one team
        public ICollection<Student>? TeamMembers { get; set; }                              // list of students in the team

        // team can have one supervisor, and one supervisor can have many teams
        public int? SupervisorId { get; set; }

        public Supervisor? Supervisor { get; set; }                                             // supvisor of the team

        // one team can have many requests to join
        public ICollection<TeamJoinRequest>? JoinRequests { get; set; }                         // list of requests to join the team

        // one team can have many projects, and one project can have one team
        public ICollection<ProjectIdea> ProjectIdeas { get; set; } = new List<ProjectIdea>();   // list of projects in the team

        // one team can have many tasks, and one task can have one team
        public ICollection<Task> Tasks { get; set; } = new List<Task>();                        // list of tasks in the team
    }
}