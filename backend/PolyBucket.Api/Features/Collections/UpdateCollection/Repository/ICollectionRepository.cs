using PolyBucket.Api.Features.Collections.Domain;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.UpdateCollection.Repository
{
    public interface ICollectionRepository
    {
        Task<Collection?> GetCollectionByIdAsync(Guid id);
        Task<Collection> UpdateCollectionAsync(Collection collection);
    }
} 