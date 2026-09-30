using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Email.TestEmailConfiguration.Repository;

public interface ITestEmailConfigurationRepository
{
    Task RecordSuccessfulTestAsync(DateTime testedAt, CancellationToken cancellationToken = default);
}
