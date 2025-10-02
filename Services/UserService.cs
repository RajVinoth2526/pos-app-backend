using ClientAppPOSWebAPI.Data;
using ClientAppPOSWebAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace ClientAppPOSWebAPI.Services
{
    public class UserService
    {
        private readonly POSDbContext _context;

        public UserService(POSDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<PagedResult<User>> GetAllUsersAsync(UserFilterDto filters)
        {
            var query = _context.Users.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(filters.Username))
                query = query.Where(u => u.Username.ToLower().Contains(filters.Username.ToLower()));

            if (!string.IsNullOrEmpty(filters.Email))
                query = query.Where(u => u.Email.ToLower().Contains(filters.Email.ToLower()));

            if (!string.IsNullOrEmpty(filters.Role) && filters.Role.ToLower() != "all")
                query = query.Where(u => u.Role == filters.Role);

            if (filters.IsActive.HasValue)
                query = query.Where(u => u.IsActive == filters.IsActive.Value);

            if (filters.StartDate.HasValue)
                query = query.Where(u => u.CreatedAt >= filters.StartDate.Value);

            if (filters.EndDate.HasValue)
                query = query.Where(u => u.CreatedAt <= filters.EndDate.Value);

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var items = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((filters.Page - 1) * filters.PageSize)
                .Take(filters.PageSize)
                .ToListAsync();

            return new PagedResult<User>
            {
                Items = items ?? new List<User>(),
                TotalCount = totalCount,
                Page = filters.Page,
                PageSize = filters.PageSize
            };
        }

        public async Task<User> CreateUserAsync(User user)
        {
            // Hash the password
            user.PasswordHash = HashPassword(user.PasswordHash);
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            user.LastPasswordChangeDate = DateTime.UtcNow;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<User?> UpdateUserAsync(int id, UserUpdateDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return null;

            // Update properties if provided
            if (dto.FirstName != null)
                user.FirstName = dto.FirstName;

            if (dto.LastName != null)
                user.LastName = dto.LastName;

            if (dto.Email != null)
                user.Email = dto.Email;

            if (dto.PhoneNumber != null)
                user.PhoneNumber = dto.PhoneNumber;

            if (dto.Role != null)
                user.Role = dto.Role;

            if (dto.IsActive.HasValue)
                user.IsActive = dto.IsActive.Value;

            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            // Verify current password
            if (!VerifyPassword(currentPassword, user.PasswordHash))
                return false;

            // Hash and update new password
            user.PasswordHash = HashPassword(newPassword);
            user.LastPasswordChangeDate = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ResetPasswordAsync(string email, string newPassword)
        {
            var user = await GetUserByEmailAsync(email);
            if (user == null)
                return false;

            // Hash and update new password
            user.PasswordHash = HashPassword(newPassword);
            user.LastPasswordChangeDate = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateLoginInfoAsync(int userId, string? ipAddress = null, string? userAgent = null)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            user.LastLoginDate = DateTime.UtcNow;
            user.FailedLoginAttempts = 0;
            user.AccountLockedUntil = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IncrementFailedLoginAttemptsAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            user.FailedLoginAttempts = (user.FailedLoginAttempts ?? 0) + 1;

            // Lock account after 5 failed attempts for 15 minutes
            if (user.FailedLoginAttempts >= 5)
            {
                user.AccountLockedUntil = DateTime.UtcNow.AddMinutes(15);
            }

            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsAccountLockedAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return true;

            if (user.AccountLockedUntil.HasValue && user.AccountLockedUntil.Value > DateTime.UtcNow)
                return true;

            return false;
        }

        public async Task<UserSession> CreateUserSessionAsync(int userId, string token, DateTime expiresAt, string? ipAddress = null, string? userAgent = null)
        {
            var session = new UserSession
            {
                UserId = userId,
                Token = token,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                IsActive = true,
                IpAddress = ipAddress,
                UserAgent = userAgent
            };

            _context.UserSessions.Add(session);
            await _context.SaveChangesAsync();

            return session;
        }

        public async Task<UserSession?> GetUserSessionAsync(string token)
        {
            return await _context.UserSessions
                .FirstOrDefaultAsync(s => s.Token == token && s.IsActive && s.ExpiresAt > DateTime.UtcNow);
        }

        public async Task<bool> DeactivateUserSessionAsync(string token)
        {
            var session = await GetUserSessionAsync(token);
            if (session == null)
                return false;

            session.IsActive = false;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeactivateAllUserSessionsAsync(int userId)
        {
            var sessions = await _context.UserSessions
                .Where(s => s.UserId == userId && s.IsActive)
                .ToListAsync();

            foreach (var session in sessions)
            {
                session.IsActive = false;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CleanupExpiredSessionsAsync()
        {
            var expiredSessions = await _context.UserSessions
                .Where(s => s.ExpiresAt <= DateTime.UtcNow)
                .ToListAsync();

            _context.UserSessions.RemoveRange(expiredSessions);
            await _context.SaveChangesAsync();

            return true;
        }

        // Password hashing methods
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            var hashedPassword = HashPassword(password);
            return hashedPassword == hash;
        }
    }
}
