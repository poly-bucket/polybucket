using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Common.Email;
using PolyBucket.Api.Features.Email.Domain;

namespace PolyBucket.Api.Features.Email.GetEmailSettings.Domain;

public class GetEmailSettingsService(IEmailSettingsResolver settingsResolver) : IGetEmailSettingsService
{
    public async Task<EmailSettingsDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var settings = await settingsResolver.GetEffectiveSettingsAsync(cancellationToken);
        return EmailSettingsDto.From(settings);
    }
}
