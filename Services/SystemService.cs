using ClientAppPOSWebAPI.Data;
using ClientAppPOSWebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ClientAppPOSWebAPI.Services
{
    public class SystemService
    {
        private readonly POSDbContext _context;

        public SystemService(POSDbContext context)
        {
            _context = context;
        }
        public async Task<ThemeSettings> AddThemeSettingsAsync(ThemeSettings theme)
        {
            if (theme == null)
            {
                return null;  // Return null if the product is invalid (optional).
            }

            await _context.ThemeSettings.AddAsync(theme);
            await _context.SaveChangesAsync();
            return theme;  // Return the created product with the generated Id.
        }

        public async Task<ThemeSettings> GetThemeSettingsByIdAsync(int id)
        {
            return await _context.ThemeSettings
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<ThemeSettings> UpdateThemeSettingsAsync(int id, UpdateThemeSettingsDto dto)
        {
            ThemeSettings theme = await _context.ThemeSettings.FindAsync(id);
            if (theme == null)
                return null;

            if (dto.BackGroundColor != null) theme.BackgroundColor = dto.BackGroundColor;
            if (dto.PrimaryColor != null) theme.PrimaryColor = dto.PrimaryColor;
            if (dto.SecondaryColor != null) theme.SecondaryColor = dto.SecondaryColor;
            if (dto.FontStyle != null) theme.FontStyle = dto.FontStyle;

            await _context.SaveChangesAsync();
            return theme;
        }

        public async Task<IEnumerable<ThemeSettings>> GetAllSystemSettings()
        {
            return await _context.ThemeSettings.ToListAsync();
        }

        public async Task<Profile> AddProfileAsync(Profile profile)
        {
            if (profile == null)
            {
                return null;  // Return null if the product is invalid (optional).
            }

            await _context.Profiles.AddAsync(profile);
            await _context.SaveChangesAsync();
            return profile;  // Return the created product with the generated Id.
        }

        public async Task<Profile> GetProfileByAsync(int id)
        {
            return await _context.Profiles
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<Profile> UpdateProfileAsync(int id, UpdateProfileDto dto)
        {
            Profile profile = await _context.Profiles.FindAsync(id);
            if (profile == null)
                return null;

            if (dto.BusinessName != null) profile.BusinessName = dto.BusinessName;
            if (dto.OwnerName != null) profile.OwnerName = dto.OwnerName;
            if (dto.PhoneNumber != null) profile.PhoneNumber = dto.PhoneNumber;
            if (dto.Email != null) profile.Email = dto.Email;

            await _context.SaveChangesAsync();
            return profile;
        }

        public async Task<IEnumerable<Profile>> GetAllProfilesAsync()
        {
            return await _context.Profiles.ToListAsync();
        }
    }

}
