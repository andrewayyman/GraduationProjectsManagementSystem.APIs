using System.ComponentModel.DataAnnotations;

namespace Graduation_Project_Management.DTOs.TasksDTOs
{
    public class SubmitTaskDto
    {
        // date 
        // public DateTime SubmissionDate { get; set; } = DateTime.UtcNow; // default to current date and time
        //public DateTime? SubmissionDate { get; set; } = DateTime.UtcNow; // default to current date and time, can be null if not provided

        public IFormFile? File { get; set; }

        [StringLength(500, ErrorMessage = "Comments cannot exceed 500 characters")]
        public string? Comments { get; set; }

        [Url(ErrorMessage = "Invalid GitHub repository URL")]
        [StringLength(200, ErrorMessage = "Repository link cannot exceed 200 characters")]
        public string? RepoLink { get; set; }


    }
}
