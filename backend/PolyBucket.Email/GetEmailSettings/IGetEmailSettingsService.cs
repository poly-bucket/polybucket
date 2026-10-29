using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Email.Domain;

namespace PolyBucket.Email.GetEmailSettings;

public interface IGetEmailSettingsService
{
    Task<EmailSettingsDto> GetAsync(CancellationToken cancellationToken = default);
}
