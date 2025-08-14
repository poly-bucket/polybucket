using PolyBucket.Api.Features.SystemSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.Repository;
using System.Net.Http;
using System.Text.Json;

namespace PolyBucket.Api.Features.SystemSettings.Services
{
    public class FontAwesomeSettingsService : IFontAwesomeSettingsService
    {
        private readonly IFontAwesomeSettingsRepository _repository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<FontAwesomeSettingsService> _logger;

        public FontAwesomeSettingsService(
            IFontAwesomeSettingsRepository repository,
            IHttpClientFactory httpClientFactory,
            ILogger<FontAwesomeSettingsService> logger)
        {
            _repository = repository;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<FontAwesomeSettings> GetSettingsAsync()
        {
            try
            {
                var settings = await _repository.GetSettingsAsync();
                if (settings == null)
                {
                    // Create default settings if none exist
                    settings = new FontAwesomeSettings();
                    await _repository.CreateSettingsAsync(settings);
                }
                return settings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting FontAwesome settings");
                // Return default settings on error
                return new FontAwesomeSettings();
            }
        }

        public async Task<FontAwesomeSettings> UpdateSettingsAsync(FontAwesomeSettings settings)
        {
            try
            {
                var existingSettings = await _repository.GetSettingsAsync();
                if (existingSettings == null)
                {
                    await _repository.CreateSettingsAsync(settings);
                }
                else
                {
                    existingSettings.IsProEnabled = settings.IsProEnabled;
                    existingSettings.ProLicenseKey = settings.ProLicenseKey;
                    existingSettings.ProKitUrl = settings.ProKitUrl;
                    existingSettings.UseProIcons = settings.UseProIcons;
                    existingSettings.FallbackToFree = settings.FallbackToFree;
                    
                    await _repository.UpdateSettingsAsync(existingSettings);
                }
                
                return await _repository.GetSettingsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating FontAwesome settings");
                throw;
            }
        }

        public async Task<bool> TestLicenseKeyAsync(string licenseKey)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(licenseKey))
                {
                    return false;
                }

                // TODO: Implement actual FontAwesome license validation
                // For now, we'll do a basic format check and simulate validation
                
                // Basic format validation (FontAwesome Pro keys are typically 32 characters)
                if (licenseKey.Length != 32)
                {
                    return false;
                }

                // Simulate API call to FontAwesome for license validation
                var isValid = await ValidateLicenseWithFontAwesomeAsync(licenseKey);
                
                // Update settings with validation result
                var settings = await GetSettingsAsync();
                settings.IsLicenseValid = isValid;
                settings.LastLicenseCheck = DateTime.UtcNow;
                settings.LicenseErrorMessage = isValid ? null : "Invalid license key";
                
                if (isValid)
                {
                    settings.IsProEnabled = true;
                    settings.ProLicenseKey = licenseKey;
                }
                
                await _repository.UpdateSettingsAsync(settings);
                
                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing FontAwesome license key");
                return false;
            }
        }

        public async Task<bool> IsProEnabledAsync()
        {
            try
            {
                var settings = await GetSettingsAsync();
                return settings.IsProEnabled && settings.IsLicenseValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if FontAwesome Pro is enabled");
                return false;
            }
        }

        private async Task<bool> ValidateLicenseWithFontAwesomeAsync(string licenseKey)
        {
            try
            {
                // TODO: Replace with actual FontAwesome API endpoint
                // This is a placeholder implementation
                
                // Simulate network delay
                await Task.Delay(1000);
                
                // For now, accept any 32-character key as valid
                // In production, this should make an actual API call to FontAwesome
                return licenseKey.Length == 32 && !licenseKey.Contains("invalid");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating license with FontAwesome");
                return false;
            }
        }
    }
}
