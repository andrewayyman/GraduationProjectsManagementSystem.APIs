using Domain.Entities;

namespace Graduation_Project_Management.DTOs.SupervisorDTOs
{
    public class SupervisorDto
    {
        public int Id { get; set; }                                                                    // unique id for the supervisor
        public string FirstName { get; set; }                                                          // first name of the supervisor
        public string LastName { get; set; }                                                           // last name of the supervisor
        public string Email { get; set; }                                                             // email of the supervisor
        public string? PhoneNumber { get; set; }                                                       // phone number of the supervisor
        public string? Department { get; set; }                                                        // department of the supervisor
        public string? ProfilePictureUrl { get; set; }                                                 // profile picture of the supervisor
        public int MaxAssignedTeams { get; set; } = 4;                                                 // max number of teams assigned to the supervisor
        public List<string>? PreferredTechnologies { get; set; }                      // list of technologies that the supervisor is familiar with
        public ICollection<TeamDto>? SupervisedTeams { get; set; } = new List<TeamDto>();                    // teams supervised by the supervisor
    }
}