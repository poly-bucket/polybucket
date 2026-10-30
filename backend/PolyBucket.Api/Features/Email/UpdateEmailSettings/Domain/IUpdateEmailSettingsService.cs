using PolyBucket.Email;
using PolyBucket.Email.Domain;
using PolyBucket.Email.UpdateEmailSettings;

namespace PolyBucket.Api.Features.Email.UpdateEmailSettings.Domain;

public interface IUpdateEmailSettingsService
{
    Task<EmailSettingsDto> UpdateAsync(EmailSettingsUpdate update, CancellationToken cancellationToken = default);
}
