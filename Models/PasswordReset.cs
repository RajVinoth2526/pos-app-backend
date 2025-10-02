using System.ComponentModel.DataAnnotations;

namespace ClientAppPOSWebAPI.Models
{
    public class PasswordResetToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }

        // Navigation property
        public User User { get; set; } = null!;
    }

    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }

    public class PasswordResetRequestDto
    {
        [Required]
        public string Token { get; set; } = null!;
        
        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public string NewPassword { get; set; } = null!;
        
        [Required]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = null!;
    }

    public class ValidateResetTokenDto
    {
        [Required]
        public string Token { get; set; } = null!;
    }

    public class PasswordResetResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public string? ResetToken { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
