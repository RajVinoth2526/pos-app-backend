using ClientAppPOSWebAPI.Models;
using ClientAppPOSWebAPI.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ClientAppPOSWebAPI.Managers
{
    public class UserManager
    {
        private readonly UserService _userService;
        private readonly IConfiguration _configuration;

        public UserManager(UserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;
        }

        public async Task<User?> GetUserAsync(int id)
        {
            return await _userService.GetUserByIdAsync(id);
        }

        public async Task<PagedResult<User>> GetAllUsersAsync(UserFilterDto filters)
        {
            return await _userService.GetAllUsersAsync(filters);
        }

        public async Task<UserResponseDto?> RegisterUserAsync(RegisterDto registerDto)
        {
            // Validate passwords match
            if (registerDto.Password != registerDto.ConfirmPassword)
                throw new InvalidOperationException("Passwords do not match");

            // Check if username already exists
            var existingUser = await _userService.GetUserByUsernameAsync(registerDto.Username);
            if (existingUser != null)
                throw new InvalidOperationException("Username already exists");

            // Check if email already exists
            var existingEmail = await _userService.GetUserByEmailAsync(registerDto.Email);
            if (existingEmail != null)
                throw new InvalidOperationException("Email already exists");

            // Validate password strength
            if (!IsPasswordStrong(registerDto.Password))
                throw new InvalidOperationException("Password must be at least 8 characters long and contain uppercase, lowercase, number, and special character");

            // Create new user
            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = registerDto.Password, // Will be hashed in service
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                PhoneNumber = registerDto.PhoneNumber,
                Role = registerDto.Role
            };

            var createdUser = await _userService.CreateUserAsync(user);

            return MapToUserResponseDto(createdUser);
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginDto loginDto, string? ipAddress = null, string? userAgent = null)
        {
            // Get user by username
            var user = await _userService.GetUserByUsernameAsync(loginDto.Username);
            if (user == null)
            {
                // Increment failed login attempts for the username if it exists
                var existingUser = await _userService.GetUserByUsernameAsync(loginDto.Username);
                if (existingUser != null)
                {
                    await _userService.IncrementFailedLoginAttemptsAsync(existingUser.Id);
                }
                return null;
            }

            // Check if account is locked
            if (await _userService.IsAccountLockedAsync(user.Id))
                throw new InvalidOperationException("Account is temporarily locked due to multiple failed login attempts");

            // Verify password
            if (!VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                await _userService.IncrementFailedLoginAttemptsAsync(user.Id);
                return null;
            }

            // Check if user is active
            if (!user.IsActive)
                throw new InvalidOperationException("Account is deactivated");

            // Generate JWT token
            var token = GenerateJwtToken(user);
            var expiresAt = DateTime.UtcNow.AddHours(24); // 24 hour expiration

            // Create user session
            await _userService.CreateUserSessionAsync(user.Id, token, expiresAt, ipAddress, userAgent);

            // Update login info
            await _userService.UpdateLoginInfoAsync(user.Id, ipAddress, userAgent);

            return new LoginResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt,
                User = MapToUserResponseDto(user)
            };
        }

        public async Task<bool> LogoutAsync(string token)
        {
            return await _userService.DeactivateUserSessionAsync(token);
        }

        public async Task<bool> LogoutAllSessionsAsync(int userId)
        {
            return await _userService.DeactivateAllUserSessionsAsync(userId);
        }

        public async Task<UserResponseDto?> UpdateUserAsync(int id, UserUpdateDto dto)
        {
            var updatedUser = await _userService.UpdateUserAsync(id, dto);
            if (updatedUser == null)
                return null;

            return MapToUserResponseDto(updatedUser);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            return await _userService.DeleteUserAsync(id);
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            // Validate passwords match
            if (dto.NewPassword != dto.ConfirmNewPassword)
                throw new InvalidOperationException("New passwords do not match");

            // Validate password strength
            if (!IsPasswordStrong(dto.NewPassword))
                throw new InvalidOperationException("Password must be at least 8 characters long and contain uppercase, lowercase, number, and special character");

            return await _userService.ChangePasswordAsync(userId, dto.CurrentPassword, dto.NewPassword);
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto, string newPassword)
        {
            // Validate password strength
            if (!IsPasswordStrong(newPassword))
                throw new InvalidOperationException("Password must be at least 8 characters long and contain uppercase, lowercase, number, and special character");

            return await _userService.ResetPasswordAsync(dto.Email, newPassword);
        }

        public async Task<UserResponseDto?> GetCurrentUserAsync(string token)
        {
            var session = await _userService.GetUserSessionAsync(token);
            if (session == null)
                return null;

            var user = await _userService.GetUserByIdAsync(session.UserId);
            if (user == null)
                return null;

            return MapToUserResponseDto(user);
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            var session = await _userService.GetUserSessionAsync(token);
            return session != null;
        }

        // Helper methods
        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "YourSuperSecretKey123!@#"));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "YourApp",
                audience: _configuration["Jwt:Audience"] ?? "YourAppUsers",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private bool VerifyPassword(string password, string hash)
        {
            // This would typically use a proper password hashing library like BCrypt
            // For now, using simple SHA256 comparison
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var hashedPassword = Convert.ToBase64String(hashedBytes);
                return hashedPassword == hash;
            }
        }

        private bool IsPasswordStrong(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8)
                return false;

            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

            return hasUpper && hasLower && hasDigit && hasSpecial;
        }

        private UserResponseDto MapToUserResponseDto(User user)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                IsActive = user.IsActive,
                LastLoginDate = user.LastLoginDate,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }
    }
}
