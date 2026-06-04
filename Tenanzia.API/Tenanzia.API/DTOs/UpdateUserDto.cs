namespace Tenanzia.API.DTOs
{
    public class UpdateUserDto
    {
        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
        public string? Password { get; set; }
    }
}
