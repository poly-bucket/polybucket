using PolyBucket.Email;
using PolyBucket.Email.Domain;
using PolyBucket.Email.UpdateEmailSettings;
using PolyBucket.Api.Features.Email.UpdateEmailSettings.Repository;

namespace PolyBucket.Api.Features.Email.UpdateEmailSettings.Domain;

public class UpdateEmailSettingsService(Repository.IUpdateEmailSettingsRepository repository) : IUpdateEmailSettingsService
{
    public Task<EmailSettingsDto> UpdateAsync(EmailSettingsUpdate update, CancellationToken cancellationToken = default) =>
        repository.UpdateAsync(update, cancellationToken);
}
