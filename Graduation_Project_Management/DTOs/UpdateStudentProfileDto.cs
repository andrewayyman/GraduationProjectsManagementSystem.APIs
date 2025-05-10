using System.ComponentModel.DataAnnotations;

namespace Graduation_Project_Management.DTOs
{
    public class UpdateStudentProfileDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        [Phone]
        public string? PhoneNumber { get; set; }

        public string? Department { get; set; }
        public string? Level { get; set; }
        [Range(0,4.0)]
        public double? Gpa { get; set; }

        public List<string>? TechStack { get; set; }

        [Url]
        public string? GithubProfile { get; set; }
        [Url]

        public string? LinkedInProfile { get; set; }
        public string? MainRole { get; set; }
        public string? SecondaryRole { get; set; }

        public IFormFile? ProfilePictureUrl { get; set; }
    }
}
