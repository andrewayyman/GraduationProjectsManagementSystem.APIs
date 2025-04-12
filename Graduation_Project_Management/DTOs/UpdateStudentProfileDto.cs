namespace Graduation_Project_Management.DTOs
{
    public class UpdateStudentProfileDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }

        public string? Department { get; set; }
        public string? Level { get; set; }
        public double? Gpa { get; set; }

        public List<string>? TechStack { get; set; }

        public string? GitHubProfile { get; set; }
        public string? LinkedInProfile { get; set; }
        public string? MainRole { get; set; }
        public string? SecondaryRole { get; set; }
    }
}
