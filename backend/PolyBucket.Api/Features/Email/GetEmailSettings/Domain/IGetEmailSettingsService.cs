using PolyBucket.Email.Domain;

namespace PolyBucket.Api.Features.Email.GetEmailSettings.Domain;

public interface IGetEmailSettingsService
{
    Task<EmailSettingsDto> GetAsync(CancellationToken cancellationToken = default);
}
