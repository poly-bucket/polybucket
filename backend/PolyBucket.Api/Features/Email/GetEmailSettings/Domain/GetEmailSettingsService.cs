using PolyBucket.Email.Domain;

using PolyBucket.Api.Features.Email.GetEmailSettings.Repository;

namespace PolyBucket.Api.Features.Email.GetEmailSettings.Domain;

public class GetEmailSettingsService(IGetEmailSettingsRepository repository) : IGetEmailSettingsService
{
    public Task<EmailSettingsDto> GetAsync(CancellationToken cancellationToken = default) => repository.GetAsync(cancellationToken);
}
