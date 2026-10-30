using PolyBucket.Email.TestEmailConfiguration;
using PolyBucket.Api.Features.Email.TestEmailConfiguration.Repository;

namespace PolyBucket.Api.Features.Email.TestEmailConfiguration.Domain;

public class TestEmailConfigurationService(Repository.ITestEmailConfigurationRepository repository) : ITestEmailConfigurationService
{
    public Task<EmailTestResult> TestAsync(string recipient, CancellationToken cancellationToken = default) =>
        repository.TestAsync(recipient, cancellationToken);
}
