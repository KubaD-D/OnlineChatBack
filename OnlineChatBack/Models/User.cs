namespace OnlineChatBack.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public required string PasswordHash { get; set; }   
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }
    }
}
    