using System.ComponentModel.DataAnnotations;

namespace Graduation_Project_Management.DTOs.AuthDTOs
{
    public class LoginDto
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}