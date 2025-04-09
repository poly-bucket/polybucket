using PolyBucket.Api.Features.Collections.Domain;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.CreateCollection.Repository
{
    public interface ICollectionRepository
    {
        Task<Collection> CreateCollectionAsync(Collection collection);
    }
} 