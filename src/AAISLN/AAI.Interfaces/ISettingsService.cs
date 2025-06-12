using AAI.Models;

namespace AAI.Interfaces;

public interface ISettingsService
{
    Task<bool> UpdateAsync(AppSettings settings);
    Task<AppSettings> GetAsync(string settingsId);
}