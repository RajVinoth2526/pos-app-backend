using ClientAppPOSWebAPI.Models;
using ClientAppPOSWebAPI.Services;

namespace ClientAppPOSWebAPI.Managers
{
    public class SystemManager
    {
        private readonly SystemService _systemService;

        public SystemManager(SystemService systemService)
        {
            _systemService = systemService;
        }

        public async Task<ThemeSettings> AddThemeSettingsAsync(ThemeSettings theme)
        {
            // Add any business logic here if needed
            return await _systemService.AddThemeSettingsAsync(theme);
        }

        public async Task<ThemeSettings> GetThemeSettingsByAsync(int id)
        {
            return await _systemService.GetThemeSettingsByIdAsync(id);
        }

        public async Task<ThemeSettings> UpdateThemeSettingsAsync(int id, UpdateThemeSettingsDto dto)
        {
            return await _systemService.UpdateThemeSettingsAsync(id, dto);
        }

        public async Task<IEnumerable<ThemeSettings>> GetAllThemeSettingsAsync()
        {
            return await _systemService.GetAllSystemSettings();
        }

        public async Task<Profile> AddProfileAsync(Profile profile)
        {
            // Add any business logic here if needed
            return await _systemService.AddProfileAsync(profile);
        }

        public async Task<Profile> GetProfileByAsync(int id)
        {
            return await _systemService.GetProfileByAsync(id);
        }

        public async Task<Profile> UpdateProfileAsync(int id, UpdateProfileDto dto)
        {
            return await _systemService.UpdateProfileAsync(id, dto);
        }

        public async Task<IEnumerable<Profile>> GetAllProfilesAsync()
        {
            return await _systemService.GetAllProfilesAsync();
        }


    }
}
