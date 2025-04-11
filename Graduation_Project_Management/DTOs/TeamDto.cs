namespace Graduation_Project_Management.DTOs
{
    public class TeamDto
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string TeamDepartment { get; set; }
        public bool IsOpenToJoin { get; set; }
        public int MaxMembers { get; set; }
        public List<string>? TechStack { get; set; }
    }
}
