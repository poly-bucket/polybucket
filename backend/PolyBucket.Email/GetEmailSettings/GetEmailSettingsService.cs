using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Email;
using PolyBucket.Email.Domain;

namespace PolyBucket.Email.GetEmailSettings;

public class GetEmailSettingsService(IEmailSettingsResolver settingsResolver) : IGetEmailSettingsService
{
    public async Task<EmailSettingsDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var settings = await settingsResolver.GetEffectiveSettingsAsync(cancellationToken);
        return EmailSettingsDto.From(settings);
    }
}
