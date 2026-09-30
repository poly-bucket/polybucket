using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Common.Email;
using PolyBucket.Api.Features.Email.Domain;

namespace PolyBucket.Api.Features.Email.UpdateEmailSettings.Domain;

public sealed record EmailSettingsUpdate(
    EmailTransportKind Transport,
    string SmtpHost,
    int SmtpPort,
    EmailSecurityMode SmtpSecurity,
    string SmtpUsername,
    string? SmtpPassword,
    bool ClearPassword,
    bool AllowInvalidCertificates,
    string FromAddress,
    string FromName,
    string? ReplyTo,
    string PublicBaseUrl,
    bool RequireEmailVerification);

public interface IUpdateEmailSettingsService
{
    Task<EmailSettingsDto> UpdateAsync(EmailSettingsUpdate update, CancellationToken cancellationToken = default);
}
