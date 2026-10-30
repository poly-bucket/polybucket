using PolyBucket.Email;
using PolyBucket.Email.Domain;
using PolyBucket.Email.UpdateEmailSettings;

namespace PolyBucket.Api.Features.Email.UpdateEmailSettings.Repository;

public interface IUpdateEmailSettingsRepository
{
    Task<EmailSettingsDto> UpdateAsync(EmailSettingsUpdate update, CancellationToken cancellationToken = default);
}
