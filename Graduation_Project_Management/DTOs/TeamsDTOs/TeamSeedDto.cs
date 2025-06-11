namespace Graduation_Project_Management.DTOs.TeamsDTOs
{
    public class TeamSeedDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string TeamDepartment { get; set; }
        public List<string> TechStack { get; set; }
        public List<string> MembersEmails { get; set; }
    }
}
