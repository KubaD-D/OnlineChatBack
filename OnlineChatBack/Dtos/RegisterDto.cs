using System.ComponentModel.DataAnnotations;

namespace OnlineChatBack.Dtos
{
    public class RegisterDto
    {
        public required string Username { get; set; }
        public required string Password { get; set; }

        [EmailAddress(ErrorMessage = "Email address is not valid")]
        public required string Email { get; set; }
    }
}
