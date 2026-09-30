using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Email.UpdateEmailSettings.Repository;

public interface IUpdateEmailSettingsRepository
{
    Task UpsertAsync(IReadOnlyDictionary<string, string> values, CancellationToken cancellationToken = default);
}
