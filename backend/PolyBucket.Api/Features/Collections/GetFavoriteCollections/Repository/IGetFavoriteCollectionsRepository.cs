using PolyBucket.Api.Features.Collections.GetFavoriteCollections.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.GetFavoriteCollections.Repository
{
    public interface IGetFavoriteCollectionsRepository
    {
        Task<IEnumerable<GetFavoriteCollectionsResponse>> GetFavoriteCollectionsByUserIdAsync(Guid userId);
    }
}
