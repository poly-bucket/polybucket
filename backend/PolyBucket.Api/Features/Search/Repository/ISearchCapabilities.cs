using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Database.Persistence;
using PolyBucket.Api.Features.Search.Domain;

namespace PolyBucket.Api.Features.Search.Repository;

public interface ISearchCapabilities
{
    Task<SearchTextMode> GetModeAsync(PolyBucketDbContext context, CancellationToken cancellationToken = default);

    void Invalidate();
}
