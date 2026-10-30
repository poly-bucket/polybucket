using PolyBucket.Email.Domain;
using Inner = PolyBucket.Email.GetEmailSettings.IGetEmailSettingsService;

namespace PolyBucket.Api.Features.Email.GetEmailSettings.Repository;

public interface IGetEmailSettingsRepository
{
    Task<EmailSettingsDto> GetAsync(CancellationToken cancellationToken = default);
}

public class GetEmailSettingsRepository(Inner inner) : IGetEmailSettingsRepository
{
    public Task<EmailSettingsDto> GetAsync(CancellationToken cancellationToken = default) => inner.GetAsync(cancellationToken);
}
