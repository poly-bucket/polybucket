namespace PolyBucket.Api.Features.SystemSettings.TestFontAwesomeLicense.Repository;

public interface ITestFontAwesomeLicenseRepository
{
    Task<bool> ValidateLicenseAsync(string licenseKey, CancellationToken cancellationToken = default);
}
