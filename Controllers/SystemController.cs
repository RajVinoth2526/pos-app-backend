using ClientAppPOSWebAPI.Managers;
using ClientAppPOSWebAPI.Models;
using ClientAppPOSWebAPI.Success;
using Microsoft.AspNetCore.Mvc;

namespace ClientAppPOSWebAPI.Controllers
{
    [Route("api/system")]
    [ApiController]
    public class SystemController : ControllerBase
    {
        private readonly SystemManager _systemManager;

        public SystemController(SystemManager systemanager)
        {
            _systemManager = systemanager;
        }

        [Route("theme-settings")]
        [HttpPost]
        public async Task<IActionResult> CreateThemeSettings([FromBody] ThemeSettings product)
        {
            if (product == null)
            {
                return BadRequest(Result.FailureResult("theme settings data is required"));
            }

            // Call the manager to add the product
            var systemSettings = await _systemManager.AddThemeSettingsAsync(product);

            if (systemSettings == null)
            {
                return StatusCode(500, Result.FailureResult("An error occurred while adding the theme settings"));
            }

            return CreatedAtAction(nameof(GetThemeSettings), new { id = systemSettings.Id }, Result.SuccessResult(systemSettings));
        }

        [Route("theme-settings/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetThemeSettings(int id)
        {
            var product = await _systemManager.GetThemeSettingsByAsync(id);

            if (product == null)
            {
                return NotFound(Result.FailureResult("Theme settings not found"));
            }

            return Ok(Result.SuccessResult(product));
        }

        [Route("theme-settings")]
        [HttpGet]
        public async Task<IActionResult> GetThemeSettings()
        {
            var product = await _systemManager.GetAllThemeSettingsAsync();

            if (product == null)
            {
                return NotFound(Result.FailureResult("Theme settings not found"));
            }

            return Ok(Result.SuccessResult(product));
        }

        [Route("theme-settings/{id}")]
        [HttpPatch]
        public async Task<IActionResult> PatchThemeSettings(int id, [FromBody] UpdateThemeSettingsDto dto)
        {
            if (dto == null)
                return BadRequest(Result.FailureResult("No data provided"));

            var updatedTheme = await _systemManager.UpdateThemeSettingsAsync(id, dto);

            if (updatedTheme == null)
                return NotFound(Result.FailureResult("Theme settings not found"));

            return Ok(Result.SuccessResult(updatedTheme));
        }

        [Route("profile")]
        [HttpPost]
        public async Task<IActionResult> CreateProfile([FromBody] Profile profile)
        {
            if (profile == null)
            {
                return BadRequest(Result.FailureResult("profile data is required"));
            }

            // Call the manager to add the product
            var prodileData = await _systemManager.AddProfileAsync(profile);

            if (prodileData == null)
            {
                return StatusCode(500, Result.FailureResult("An error occurred while adding the profile"));
            }

            return CreatedAtAction(nameof(GetThemeSettings), new { id = prodileData.Id }, Result.SuccessResult(prodileData));
        }

        [Route("profile/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetSingleProfile(int id)
        {
            var profile = await _systemManager.GetProfileByAsync(id);

            if (profile == null)
            {
                return NotFound(Result.FailureResult("Profile not found"));
            }

            return Ok(Result.SuccessResult(profile));
        }

        [Route("profile")]
        [HttpGet]
        public async Task<IActionResult> GetAllProfile()
        {
            var profile = await _systemManager.GetAllProfilesAsync();

            if (profile == null)
            {
                return NotFound(Result.FailureResult("profile not found"));
            }

            return Ok(Result.SuccessResult(profile));
        }

        [Route("profile/{id}")]
        [HttpPatch]
        public async Task<IActionResult> PatchProfile(int id, [FromBody] UpdateProfileDto dto)
        {
            if (dto == null)
                return BadRequest(Result.FailureResult("No data provided"));

            var updatedTheme = await _systemManager.UpdateProfileAsync(id, dto);

            if (updatedTheme == null)
                return NotFound(Result.FailureResult("Profile not found"));

            return Ok(Result.SuccessResult(updatedTheme));
        }

    }
}
