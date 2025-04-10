using System.ComponentModel.DataAnnotations;

namespace Graduation_Project_Management.DTOs.AuthDTOs
{
    public class RegisterSupervisorDto
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]

        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
