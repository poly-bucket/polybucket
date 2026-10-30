namespace PolyBucket.Api.Features.SystemSettings.TestFontAwesomeLicense.Domain;

public interface ITestFontAwesomeLicenseService
{
    Task<(bool IsValid, string Message)> TestFontAwesomeLicenseAsync(string licenseKey, CancellationToken cancellationToken = default);
}
