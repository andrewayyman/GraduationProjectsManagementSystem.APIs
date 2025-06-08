using System.ComponentModel.DataAnnotations;

namespace Graduation_Project_Management.DTOs.TasksDTOs
{
    public class SubmitTaskServiceDto
    {
        public string? FilePath { get; set; }

        [StringLength(500, ErrorMessage = "Comments cannot exceed 500 characters")]
        public string? Comments { get; set; }

        [Url(ErrorMessage = "Invalid GitHub repository URL")]
        [StringLength(200, ErrorMessage = "Repository link cannot exceed 200 characters")]
        public string? RepoLink { get; set; }

    }
}
