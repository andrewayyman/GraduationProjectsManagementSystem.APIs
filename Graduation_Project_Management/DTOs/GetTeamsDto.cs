namespace Graduation_Project_Management.DTOs
{
    public class GetTeamsDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string TeamDepartment { get; set; }
        public int MembersCount { get; set; }
        public List<string>? TechStack { get; set; }
    }
}
