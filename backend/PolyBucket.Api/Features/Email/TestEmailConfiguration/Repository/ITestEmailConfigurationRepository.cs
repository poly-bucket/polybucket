using PolyBucket.Email.TestEmailConfiguration;

namespace PolyBucket.Api.Features.Email.TestEmailConfiguration.Repository;

public interface ITestEmailConfigurationRepository
{
    Task<EmailTestResult> TestAsync(string recipient, CancellationToken cancellationToken = default);
}
