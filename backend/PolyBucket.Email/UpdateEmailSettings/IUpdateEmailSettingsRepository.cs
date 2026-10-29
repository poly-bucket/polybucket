using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Email.UpdateEmailSettings;

public interface IUpdateEmailSettingsRepository
{
    Task UpsertAsync(IReadOnlyDictionary<string, string> values, CancellationToken cancellationToken = default);
}
