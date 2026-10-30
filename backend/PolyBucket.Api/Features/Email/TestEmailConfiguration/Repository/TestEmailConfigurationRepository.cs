using PolyBucket.Email.TestEmailConfiguration;
using Inner = PolyBucket.Email.TestEmailConfiguration.ITestEmailConfigurationService;

namespace PolyBucket.Api.Features.Email.TestEmailConfiguration.Repository;

public class TestEmailConfigurationRepository(Inner inner) : ITestEmailConfigurationRepository
{
    public Task<EmailTestResult> TestAsync(string recipient, CancellationToken cancellationToken = default) =>
        inner.TestAsync(recipient, cancellationToken);
}
