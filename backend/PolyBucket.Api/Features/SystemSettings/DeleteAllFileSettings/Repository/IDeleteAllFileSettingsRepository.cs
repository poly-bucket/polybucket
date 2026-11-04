using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.SystemSettings.DeleteAllFileSettings.Repository
{
    public interface IDeleteAllFileSettingsRepository
    {
        Task<int> DeleteAllFileSettingsAndReturnCountAsync(Guid deletedById, CancellationToken cancellationToken = default);
    }
}
