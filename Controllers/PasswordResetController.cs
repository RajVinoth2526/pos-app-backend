using ClientAppPOSWebAPI.Models;
using ClientAppPOSWebAPI.Services;
using ClientAppPOSWebAPI.Success;
using Microsoft.AspNetCore.Mvc;

namespace ClientAppPOSWebAPI.Controllers
{
    [Route("api/password-reset")]
    [ApiController]
    public class PasswordResetController : ControllerBase
    {
        private readonly PasswordResetService _passwordResetService;
        private readonly ILogger<PasswordResetController> _logger;

        public PasswordResetController(PasswordResetService passwordResetService, ILogger<PasswordResetController> logger)
        {
            _passwordResetService = passwordResetService;
            _logger = logger;
        }

        // POST: api/password-reset/forgot
        [HttpPost("forgot")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            if (forgotPasswordDto == null || string.IsNullOrEmpty(forgotPasswordDto.Email))
            {
                return BadRequest(Result.FailureResult("Email is required"));
            }

            try
            {
                var result = await _passwordResetService.RequestPasswordResetAsync(forgotPasswordDto.Email);
                
                if (result.Success)
                {
                    _logger.LogInformation($"Password reset requested for email: {forgotPasswordDto.Email}");
                    
                    // In a real application, you would send the token via email
                    // For offline use, we return the token in the response
                    var response = new
                    {
                        success = true,
                        message = result.Message,
                        resetToken = result.ResetToken, // Only for offline use!
                        expiresAt = result.ExpiresAt,
                        instructions = new
                        {
                            step1 = "Copy the resetToken from this response",
                            step2 = "Use the resetToken in the reset password endpoint",
                            step3 = "The token expires in 24 hours",
                            note = "In a production system, this token would be sent via email"
                        }
                    };
                    
                    return Ok(response);
                }
                else
                {
                    return BadRequest(Result.FailureResult(result.Message));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in forgot password: {ex.Message}");
                return StatusCode(500, Result.FailureResult("An error occurred while processing your request"));
            }
        }

        // POST: api/password-reset/reset
        [HttpPost("reset")]
        public async Task<IActionResult> ResetPassword([FromBody] PasswordResetRequestDto resetPasswordDto)
        {
            if (resetPasswordDto == null)
            {
                return BadRequest(Result.FailureResult("Reset data is required"));
            }

            if (string.IsNullOrEmpty(resetPasswordDto.Token))
            {
                return BadRequest(Result.FailureResult("Reset token is required"));
            }

            if (string.IsNullOrEmpty(resetPasswordDto.NewPassword))
            {
                return BadRequest(Result.FailureResult("New password is required"));
            }

            if (resetPasswordDto.NewPassword != resetPasswordDto.ConfirmPassword)
            {
                return BadRequest(Result.FailureResult("Passwords do not match"));
            }

            if (resetPasswordDto.NewPassword.Length < 6)
            {
                return BadRequest(Result.FailureResult("Password must be at least 6 characters long"));
            }

            try
            {
                var result = await _passwordResetService.ResetPasswordAsync(resetPasswordDto.Token, resetPasswordDto.NewPassword);
                
                if (result.Success)
                {
                    _logger.LogInformation("Password reset completed successfully");
                    return Ok(Result.SuccessResult(result.Message));
                }
                else
                {
                    return BadRequest(Result.FailureResult(result.Message));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in reset password: {ex.Message}");
                return StatusCode(500, Result.FailureResult("An error occurred while resetting your password"));
            }
        }

        // POST: api/password-reset/validate-token
        [HttpPost("validate-token")]
        public async Task<IActionResult> ValidateResetToken([FromBody] ValidateResetTokenDto validateTokenDto)
        {
            if (validateTokenDto == null || string.IsNullOrEmpty(validateTokenDto.Token))
            {
                return BadRequest(Result.FailureResult("Token is required"));
            }

            try
            {
                var result = await _passwordResetService.ValidateResetTokenAsync(validateTokenDto.Token);
                
                if (result.Success)
                {
                    return Ok(Result.SuccessResult(new
                    {
                        message = result.Message,
                        expiresAt = result.ExpiresAt,
                        isValid = true
                    }));
                }
                else
                {
                    return BadRequest(Result.FailureResult(result.Message));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error validating reset token: {ex.Message}");
                return StatusCode(500, Result.FailureResult("An error occurred while validating the token"));
            }
        }

        // GET: api/password-reset/cleanup (Admin only)
        [HttpGet("cleanup")]
        public async Task<IActionResult> CleanupExpiredTokens()
        {
            try
            {
                await _passwordResetService.CleanupExpiredTokensAsync();
                return Ok(Result.SuccessResult("Expired tokens cleaned up successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error cleaning up expired tokens: {ex.Message}");
                return StatusCode(500, Result.FailureResult("An error occurred while cleaning up tokens"));
            }
        }

        // GET: api/password-reset/user-tokens/{userId} (Admin only)
        [HttpGet("user-tokens/{userId}")]
        public async Task<IActionResult> GetUserResetTokens(int userId)
        {
            try
            {
                var tokens = await _passwordResetService.GetUserResetTokensAsync(userId);
                return Ok(Result.SuccessResult(tokens));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting user reset tokens: {ex.Message}");
                return StatusCode(500, Result.FailureResult("An error occurred while retrieving tokens"));
            }
        }

        // POST: api/password-reset/revoke-all/{userId} (Admin only)
        [HttpPost("revoke-all/{userId}")]
        public async Task<IActionResult> RevokeAllUserTokens(int userId)
        {
            try
            {
                var success = await _passwordResetService.RevokeAllUserTokensAsync(userId);
                
                if (success)
                {
                    return Ok(Result.SuccessResult("All reset tokens for user have been revoked"));
                }
                else
                {
                    return StatusCode(500, Result.FailureResult("Failed to revoke user tokens"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error revoking user tokens: {ex.Message}");
                return StatusCode(500, Result.FailureResult("An error occurred while revoking tokens"));
            }
        }
    }
}
