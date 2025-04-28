using Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    // student entity which represent a university student to be used in application thatt manage graduation projects for the university
    public class Student
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string? Department { get; set; }
        public string? Level { get; set; }
        public double? Gpa { get; set; }
        public string? ProfilePictureUrl { get; set; }

        public string? MainRole { get; set; }                                                        // backend developer, front end developer, designer, etc.
        public string? SecondaryRole { get; set; }
        public List<string>? TechStack { get; set; } = new List<string>();                          // list of technologies that the student is familiar with
        public string? GithubProfile { get; set; }                                                  // github profile of the student
        public string? LinkedInProfile { get; set; }

        // RelationShips

        public string UserId { get; set; }
        public AppUser User { get; set; }

        // one student can be in on team , teams can have many students
        public int? TeamId { get; set; }

        public Team? Team { get; set; }                                                             // team that the student is in

        // one student can request to joiun many teams, and one team can have many students
        public ICollection<TeamJoinRequest>? JoinRequests { get; set; }                             // list of requests to join teams

        // one student can have many tasks assigned to him, and one task can have many students
        public ICollection<Task>? AssignedTasks { get; set; } = new List<Task>();                   // list of tasks assigned to the student
    }
}