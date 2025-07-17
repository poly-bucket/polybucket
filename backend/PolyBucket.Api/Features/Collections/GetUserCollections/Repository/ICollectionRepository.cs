using PolyBucket.Api.Features.Collections.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.GetUserCollections.Repository
{
    public interface ICollectionRepository
    {
        Task<IEnumerable<Collection>> GetCollectionsByUserIdAsync(Guid userId);
    }
} 