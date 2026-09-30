using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Common.Email;

namespace PolyBucket.Api.Features.Email.TestEmailConfiguration.Domain;

public sealed record EmailTestResult(bool Success, string Message, IReadOnlyList<EmailDiagnosticStageResult> Stages);

public interface ITestEmailConfigurationService
{
    Task<EmailTestResult> TestAsync(string recipient, CancellationToken cancellationToken = default);
}
