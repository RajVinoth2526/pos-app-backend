namespace ClientAppPOSWebAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = "User"; // Admin, Manager, Cashier, User
        public bool IsActive { get; set; } = true;
        public DateTime? LastLoginDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastPasswordChangeDate { get; set; }
        public int? FailedLoginAttempts { get; set; } = 0;
        public DateTime? AccountLockedUntil { get; set; }
    }

    public class UserSession
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }
}
