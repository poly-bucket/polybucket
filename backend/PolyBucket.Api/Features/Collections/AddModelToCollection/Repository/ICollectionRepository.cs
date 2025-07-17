using PolyBucket.Api.Features.Collections.Domain;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.AddModelToCollection.Repository
{
    public interface ICollectionRepository
    {
        Task<Collection?> GetCollectionByIdAsync(Guid id);
        Task<bool> ModelExistsAsync(Guid modelId);
        Task<bool> IsModelInCollectionAsync(Guid collectionId, Guid modelId);
        Task AddModelToCollectionAsync(Guid collectionId, Guid modelId);
    }
} 