using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Email;

namespace PolyBucket.Email.TestEmailConfiguration;

public sealed record EmailTestResult(bool Success, string Message, IReadOnlyList<EmailDiagnosticStageResult> Stages);

public interface ITestEmailConfigurationService
{
    Task<EmailTestResult> TestAsync(string recipient, CancellationToken cancellationToken = default);
}
