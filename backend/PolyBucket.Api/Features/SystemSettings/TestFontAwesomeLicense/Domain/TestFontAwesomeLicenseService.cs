using PolyBucket.Api.Features.SystemSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.Repository;
using PolyBucket.Api.Features.SystemSettings.TestFontAwesomeLicense.Repository;

namespace PolyBucket.Api.Features.SystemSettings.TestFontAwesomeLicense.Domain;

public class TestFontAwesomeLicenseService(
    ITestFontAwesomeLicenseRepository testRepository,
    IFontAwesomeSettingsRepository settingsRepository) : ITestFontAwesomeLicenseService
{
    public async Task<(bool IsValid, string Message)> TestFontAwesomeLicenseAsync(string licenseKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(licenseKey) || licenseKey.Length != 32)
        {
            return (false, "Invalid license key");
        }

        var isValid = await testRepository.ValidateLicenseAsync(licenseKey, cancellationToken);
        var settings = await settingsRepository.GetSettingsAsync();
        if (settings != null)
        {
            settings.IsLicenseValid = isValid;
            settings.LastLicenseCheck = DateTime.UtcNow;
            settings.LicenseErrorMessage = isValid ? null : "Invalid license key";
            if (isValid)
            {
                settings.IsProEnabled = true;
                settings.ProLicenseKey = licenseKey;
            }
            await settingsRepository.UpdateSettingsAsync(settings);
        }

        return (isValid, isValid ? "License key is valid" : "Invalid license key");
    }
}
