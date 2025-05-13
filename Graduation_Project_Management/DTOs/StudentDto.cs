namespace Graduation_Project_Management.DTOs
{
    public class StudentDto
    {
        public int StudentId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Department { get; set; }
        public double? Gpa { get; set; }
        public List<string>? TechStack { get; set; }
        public string? GithubProfile { get; set; }
        public string? LinkedInProfile { get; set; }
        public string? MainRole { get; set; }
        public string? SecondaryRole { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public int? TeamId { get; set; }
        public string? TeamName { get; set; }
    }
}