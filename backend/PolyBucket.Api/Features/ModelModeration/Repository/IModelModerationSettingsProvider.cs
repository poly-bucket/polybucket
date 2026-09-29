using PolyBucket.Api.Features.ModelModeration.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ModelModeration.Repository;

public interface IModelModerationSettingsProvider
{
    Task<ModelModerationSettingsSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default);
}
