using PolyBucket.Api.Features.Collections.Domain;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.DeleteCollection.Repository
{
    public interface ICollectionRepository
    {
        Task<Collection?> GetCollectionByIdAsync(Guid id);
        Task DeleteCollectionAsync(Collection collection);
    }
} 