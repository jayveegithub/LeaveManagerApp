using System.ComponentModel.DataAnnotations;

namespace LeaveManagerApp.Web.Controllers.DTO
{
    public class RegisterDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required, MinLength(6)]
        public string Password { get; set; } = null!;

        [Required, MinLength(6)]
        public string ConfirmPassword { get; set; } = null!;

        [StringLength(200)]
        public string? FullName { get; set; }

        public string? Role { get; set; }
    }

    public class RegisterResponseDto
    {
        public string Id { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? FullName { get; set; }
    }

    public class LoginDto
    {
        [Required]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = null!;
        public int ExpiresIn { get; set; }
        public string UserId { get; set; } = null!;
        public string? FullName { get; set; }
    }
}