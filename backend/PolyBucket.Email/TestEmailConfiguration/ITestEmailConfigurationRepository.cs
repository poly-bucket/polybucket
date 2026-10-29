using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Email.TestEmailConfiguration;

public interface ITestEmailConfigurationRepository
{
    Task RecordSuccessfulTestAsync(DateTime testedAt, CancellationToken cancellationToken = default);
}
