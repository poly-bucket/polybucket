using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.DeleteAllCollections.Repository
{
    public interface IDeleteAllCollectionsRepository
    {
        Task<int> DeleteAllCollectionsAndReturnCountAsync(CancellationToken cancellationToken = default);
    }
}
