namespace Graduation_Project_Management.DTOs
{
    public class UpdateTeamDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? TeamDepartment { get; set; }
        public List<string>? TechStack { get; set; }

    }
}
