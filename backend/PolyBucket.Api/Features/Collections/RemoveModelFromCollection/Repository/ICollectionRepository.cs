using PolyBucket.Api.Features.Collections.Domain;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.RemoveModelFromCollection.Repository
{
    public interface ICollectionRepository
    {
        Task<Collection?> GetCollectionByIdAsync(Guid id);
        Task<bool> IsModelInCollectionAsync(Guid collectionId, Guid modelId);
        Task RemoveModelFromCollectionAsync(Guid collectionId, Guid modelId);
    }
} 