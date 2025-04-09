using PolyBucket.Api.Features.Collections.Domain;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.GetCollectionById.Repository
{
    public interface ICollectionRepository
    {
        Task<Collection?> GetCollectionByIdAsync(Guid id);
    }
} 