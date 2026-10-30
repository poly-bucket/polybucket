using PolyBucket.Email.TestEmailConfiguration;

namespace PolyBucket.Api.Features.Email.TestEmailConfiguration.Domain;

public interface ITestEmailConfigurationService
{
    Task<EmailTestResult> TestAsync(string recipient, CancellationToken cancellationToken = default);
}
