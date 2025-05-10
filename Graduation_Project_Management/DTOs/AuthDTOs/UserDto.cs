namespace Graduation_Project_Management.DTOs.AuthDTOs
{
    public class UserDto
    {
        public int UserId { get; set; } 
        public string Role { get; set; } 
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Token { get; set; }
    }
}