using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Supervisor
    {
        public int Id { get; set; }                                                                    // unique id for the supervisor
        public string UniversityId { get; set; }                                                       //  id for the supervisor in the university system
        public string FirstName { get; set; }                                                          // first name of the supervisor
        public string LastName { get; set; }                                                           // last name of the supervisor
        public string? Email { get; set; }                                                             // email of the supervisor
        public string? PhoneNumber { get; set; }                                                       // phone number of the supervisor
        public string? Department { get; set; }                                                        // department of the supervisor
        public string? ProfilePictureUrl { get; set; }                                                 // profile picture of the supervisor
        public List<string> PreferredTechnologies { get; set; } = new();                               // list of technologies that the supervisor is familiar with

        // RelationShips

        // one supervisor can have many teams, and one team can have one supervisor
        public ICollection<Team>? SupervisedTeams { get; set; } = new List<Team>();                    // teams supervised by the supervisor

        // one supervisor can have many projects, and one project can have one supervisor
        public ICollection<ProjectIdea>? SupervisedProjects { get; set; } = new List<ProjectIdea>();   // projects supervised by the supervisor

        // one supervisor can create many tasks , and one task can have one supervisor
        public ICollection<Task>? TasksCreated { get; set; } = new List<Task>();                       // tasks created by the supervisor
    }
}