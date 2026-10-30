using PolyBucket.Email;
using PolyBucket.Email.Domain;
using PolyBucket.Email.UpdateEmailSettings;
using Inner = PolyBucket.Email.UpdateEmailSettings.IUpdateEmailSettingsService;

namespace PolyBucket.Api.Features.Email.UpdateEmailSettings.Repository;

public class UpdateEmailSettingsRepository(Inner inner) : IUpdateEmailSettingsRepository
{
    public Task<EmailSettingsDto> UpdateAsync(EmailSettingsUpdate update, CancellationToken cancellationToken = default) =>
        inner.UpdateAsync(update, cancellationToken);
}
