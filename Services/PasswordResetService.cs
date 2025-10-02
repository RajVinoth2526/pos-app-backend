using ClientAppPOSWebAPI.Data;
using ClientAppPOSWebAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace ClientAppPOSWebAPI.Services
{
    public class PasswordResetService
    {
        private readonly POSDbContext _context;
        private readonly ILogger<PasswordResetService> _logger;
        private readonly TimeSpan _tokenExpiry = TimeSpan.FromHours(24); // Token expires in 24 hours

        public PasswordResetService(POSDbContext context, ILogger<PasswordResetService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PasswordResetResponseDto> RequestPasswordResetAsync(string email)
        {
            try
            {
                // Find user by email
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    // Return success even if user doesn't exist (security best practice)
                    return new PasswordResetResponseDto
                    {
                        Success = true,
                        Message = "If an account with that email exists, a password reset link has been sent."
                    };
                }

                // Check if user is active
                if (!user.IsActive)
                {
                    return new PasswordResetResponseDto
                    {
                        Success = false,
                        Message = "Account is deactivated. Please contact administrator."
                    };
                }

                // Generate secure reset token
                var resetToken = GenerateSecureToken();
                var expiresAt = DateTime.UtcNow.Add(_tokenExpiry);

                // Invalidate any existing reset tokens for this user
                var existingTokens = await _context.PasswordResetTokens
                    .Where(t => t.UserId == user.Id && !t.IsUsed)
                    .ToListAsync();

                foreach (var token in existingTokens)
                {
                    token.IsUsed = true;
                }

                // Create new reset token
                var passwordResetToken = new PasswordResetToken
                {
                    UserId = user.Id,
                    Token = resetToken,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = expiresAt,
                    IsUsed = false,
                    IpAddress = GetClientIpAddress(),
                    UserAgent = GetUserAgent()
                };

                _context.PasswordResetTokens.Add(passwordResetToken);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Password reset token generated for user {user.Email}");

                return new PasswordResetResponseDto
                {
                    Success = true,
                    Message = "Password reset token generated successfully. Use this token to reset your password.",
                    ResetToken = resetToken, // In a real app, this would be sent via email
                    ExpiresAt = expiresAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating password reset token: {ex.Message}");
                return new PasswordResetResponseDto
                {
                    Success = false,
                    Message = "An error occurred while processing your request. Please try again."
                };
            }
        }

        public async Task<PasswordResetResponseDto> ResetPasswordAsync(string token, string newPassword)
        {
            try
            {
                // Find valid reset token
                var resetToken = await _context.PasswordResetTokens
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.Token == token && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);

                if (resetToken == null)
                {
                    return new PasswordResetResponseDto
                    {
                        Success = false,
                        Message = "Invalid or expired reset token."
                    };
                }

                // Check if user is still active
                if (!resetToken.User.IsActive)
                {
                    return new PasswordResetResponseDto
                    {
                        Success = false,
                        Message = "Account is deactivated. Please contact administrator."
                    };
                }

                // Hash the new password
                var hashedPassword = HashPassword(newPassword);

                // Update user password
                resetToken.User.PasswordHash = hashedPassword;
                resetToken.User.LastPasswordChangeDate = DateTime.UtcNow;
                resetToken.User.UpdatedAt = DateTime.UtcNow;

                // Mark token as used
                resetToken.IsUsed = true;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Password reset successfully for user {resetToken.User.Email}");

                return new PasswordResetResponseDto
                {
                    Success = true,
                    Message = "Password has been reset successfully. You can now login with your new password."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error resetting password: {ex.Message}");
                return new PasswordResetResponseDto
                {
                    Success = false,
                    Message = "An error occurred while resetting your password. Please try again."
                };
            }
        }

        public async Task<PasswordResetResponseDto> ValidateResetTokenAsync(string token)
        {
            try
            {
                var resetToken = await _context.PasswordResetTokens
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.Token == token && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);

                if (resetToken == null)
                {
                    return new PasswordResetResponseDto
                    {
                        Success = false,
                        Message = "Invalid or expired reset token."
                    };
                }

                if (!resetToken.User.IsActive)
                {
                    return new PasswordResetResponseDto
                    {
                        Success = false,
                        Message = "Account is deactivated."
                    };
                }

                return new PasswordResetResponseDto
                {
                    Success = true,
                    Message = "Reset token is valid.",
                    ExpiresAt = resetToken.ExpiresAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error validating reset token: {ex.Message}");
                return new PasswordResetResponseDto
                {
                    Success = false,
                    Message = "An error occurred while validating the token."
                };
            }
        }

        public async Task CleanupExpiredTokensAsync()
        {
            try
            {
                var expiredTokens = await _context.PasswordResetTokens
                    .Where(t => t.ExpiresAt < DateTime.UtcNow || t.IsUsed)
                    .ToListAsync();

                if (expiredTokens.Any())
                {
                    _context.PasswordResetTokens.RemoveRange(expiredTokens);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Cleaned up {expiredTokens.Count} expired password reset tokens");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error cleaning up expired tokens: {ex.Message}");
            }
        }

        private string GenerateSecureToken()
        {
            // Generate a cryptographically secure random token
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32]; // 256 bits
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }

        private string HashPassword(string password)
        {
            // Use the same hashing method as your UserService
            // This should match the hashing method used in UserService
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private string? GetClientIpAddress()
        {
            // This would be implemented based on your HTTP context
            // For now, return null as we don't have access to HttpContext here
            return null;
        }

        private string? GetUserAgent()
        {
            // This would be implemented based on your HTTP context
            // For now, return null as we don't have access to HttpContext here
            return null;
        }

        public async Task<List<PasswordResetToken>> GetUserResetTokensAsync(int userId)
        {
            return await _context.PasswordResetTokens
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> RevokeAllUserTokensAsync(int userId)
        {
            try
            {
                var tokens = await _context.PasswordResetTokens
                    .Where(t => t.UserId == userId && !t.IsUsed)
                    .ToListAsync();

                foreach (var token in tokens)
                {
                    token.IsUsed = true;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error revoking user tokens: {ex.Message}");
                return false;
            }
        }
    }
}

