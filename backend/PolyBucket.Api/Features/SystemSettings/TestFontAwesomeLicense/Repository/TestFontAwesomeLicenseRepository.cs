namespace PolyBucket.Api.Features.SystemSettings.TestFontAwesomeLicense.Repository;

public class TestFontAwesomeLicenseRepository : ITestFontAwesomeLicenseRepository
{
    public async Task<bool> ValidateLicenseAsync(string licenseKey, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        return licenseKey.Length == 32 && !licenseKey.Contains("invalid");
    }
}
