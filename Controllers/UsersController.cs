using ClientAppPOSWebAPI.Managers;
using ClientAppPOSWebAPI.Models;
using ClientAppPOSWebAPI.Success;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ClientAppPOSWebAPI.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager _userManager;

        public UsersController(UserManager userManager)
        {
            _userManager = userManager;
        }

        // GET: api/users/test
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok("UsersController is working!");
        }

        // POST: api/users/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (registerDto == null)
            {
                return BadRequest(Result.FailureResult("Registration data is required"));
            }

            try
            {
                var user = await _userManager.RegisterUserAsync(registerDto);
                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, Result.SuccessResult(user));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(Result.FailureResult(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, Result.FailureResult("An error occurred while registering the user"));
            }
        }

        // POST: api/users/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (loginDto == null)
            {
                return BadRequest(Result.FailureResult("Login data is required"));
            }

            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

                var loginResult = await _userManager.LoginAsync(loginDto, ipAddress, userAgent);

                if (loginResult == null)
                {
                    return Unauthorized(Result.FailureResult("Invalid username or password"));
                }

                return Ok(Result.SuccessResult(loginResult));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(Result.FailureResult(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, Result.FailureResult("An error occurred during login"));
            }
        }

        // POST: api/users/logout
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(Result.FailureResult("Token is required"));
            }

            var success = await _userManager.LogoutAsync(token);
            
            if (!success)
            {
                return BadRequest(Result.FailureResult("Invalid token"));
            }

            return Ok(Result.SuccessResult("Logged out successfully"));
        }

        // POST: api/users/logout-all
        [HttpPost("logout-all")]
        [Authorize]
        public async Task<IActionResult> LogoutAllSessions()
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(Result.FailureResult("Token is required"));
            }

            var currentUser = await _userManager.GetCurrentUserAsync(token);
            if (currentUser == null)
            {
                return Unauthorized(Result.FailureResult("Invalid token"));
            }

            var success = await _userManager.LogoutAllSessionsAsync(currentUser.Id);
            
            if (!success)
            {
                return BadRequest(Result.FailureResult("Failed to logout all sessions"));
            }

            return Ok(Result.SuccessResult("All sessions logged out successfully"));
        }

        // GET: api/users/me
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(Result.FailureResult("Token is required"));
            }

            var user = await _userManager.GetCurrentUserAsync(token);
            
            if (user == null)
            {
                return Unauthorized(Result.FailureResult("Invalid token"));
            }

            return Ok(Result.SuccessResult(user));
        }

        // GET: api/users/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager,admin,manager")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _userManager.GetUserAsync(id);

            if (user == null)
            {
                return NotFound(Result.FailureResult("User not found"));
            }

            return Ok(Result.SuccessResult(user));
        }

        // GET: api/users
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,admin,manager")]
        public async Task<IActionResult> GetAllUsers([FromQuery] UserFilterDto filters)
        {
            var pagedResult = await _userManager.GetAllUsersAsync(filters);

            // Always return a valid result, even if no users found
            if (pagedResult == null)
            {
                pagedResult = new PagedResult<User>
                {
                    Items = new List<User>(),
                    TotalCount = 0,
                    Page = filters.Page,
                    PageSize = filters.PageSize
                };
            }

            return Ok(Result.SuccessResult(pagedResult));
        }

        // PATCH: api/users/{id}
        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin,Manager,admin,manager")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateDto dto)
        {
            if (dto == null)
                return BadRequest(Result.FailureResult("No data provided"));

            var updatedUser = await _userManager.UpdateUserAsync(id, dto);

            if (updatedUser == null)
                return NotFound(Result.FailureResult("User not found"));

            return Ok(Result.SuccessResult(updatedUser));
        }

        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var success = await _userManager.DeleteUserAsync(id);

            if (!success)
            {
                return NotFound(Result.FailureResult("User not found"));
            }

            return Ok(Result.SuccessResult("User deleted successfully"));
        }

        // POST: api/users/change-password
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (dto == null)
                return BadRequest(Result.FailureResult("Password change data is required"));

            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(Result.FailureResult("Token is required"));
            }

            var currentUser = await _userManager.GetCurrentUserAsync(token);
            if (currentUser == null)
            {
                return Unauthorized(Result.FailureResult("Invalid token"));
            }

            try
            {
                var success = await _userManager.ChangePasswordAsync(currentUser.Id, dto);

                if (!success)
                {
                    return BadRequest(Result.FailureResult("Current password is incorrect"));
                }

                return Ok(Result.SuccessResult("Password changed successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(Result.FailureResult(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, Result.FailureResult("An error occurred while changing password"));
            }
        }

        // POST: api/users/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (dto == null)
                return BadRequest(Result.FailureResult("Reset password data is required"));

            // In a real application, you would send an email with a reset link
            // For now, we'll just return a success message
            return Ok(Result.SuccessResult("If the email exists, a password reset link has been sent"));
        }

        // POST: api/users/validate-token
        [HttpPost("validate-token")]
        public async Task<IActionResult> ValidateToken([FromBody] string token)
        {
            if (string.IsNullOrEmpty(token))
                return BadRequest(Result.FailureResult("Token is required"));

            var isValid = await _userManager.ValidateTokenAsync(token);

            if (!isValid)
            {
                return Unauthorized(Result.FailureResult("Invalid or expired token"));
            }

            return Ok(Result.SuccessResult("Token is valid"));
        }

        // GET: api/users/role/{role}
        [HttpGet("role/{role}")]
        [Authorize(Roles = "Admin,Manager,admin,manager")]
        public async Task<IActionResult> GetUsersByRole(string role)
        {
            var filters = new UserFilterDto { Role = role };
            var pagedResult = await _userManager.GetAllUsersAsync(filters);

            // Always return a valid result, even if no users found
            if (pagedResult == null)
            {
                pagedResult = new PagedResult<User>
                {
                    Items = new List<User>(),
                    TotalCount = 0,
                    Page = filters.Page,
                    PageSize = filters.PageSize
                };
            }

            return Ok(Result.SuccessResult(pagedResult));
        }

        // GET: api/users/active
        [HttpGet("active")]
        [Authorize(Roles = "Admin,Manager,admin,manager")]
        public async Task<IActionResult> GetActiveUsers()
        {
            var filters = new UserFilterDto { IsActive = true };
            var pagedResult = await _userManager.GetAllUsersAsync(filters);

            // Always return a valid result, even if no users found
            if (pagedResult == null)
            {
                pagedResult = new PagedResult<User>
                {
                    Items = new List<User>(),
                    TotalCount = 0,
                    Page = filters.Page,
                    PageSize = filters.PageSize
                };
            }

            return Ok(Result.SuccessResult(pagedResult));
        }
    }
}
