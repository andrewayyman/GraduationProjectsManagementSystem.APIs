namespace Graduation_Project_Management.DTOs.TeamsDTOs
{
    public class TeamDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string TeamDepartment { get; set; }
        public List<string>? TechStack { get; set; }
        public List<string>? MembersEmails { get; set; }


    }
}
