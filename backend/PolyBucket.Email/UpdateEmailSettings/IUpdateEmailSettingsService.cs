using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Email;
using PolyBucket.Email.Domain;

namespace PolyBucket.Email.UpdateEmailSettings;

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
