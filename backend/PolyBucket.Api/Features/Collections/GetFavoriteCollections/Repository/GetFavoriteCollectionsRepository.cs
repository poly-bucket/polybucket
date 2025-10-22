using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Collections.GetFavoriteCollections.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.GetFavoriteCollections.Repository
{
    public class GetFavoriteCollectionsRepository : IGetFavoriteCollectionsRepository
    {
        private readonly PolyBucketDbContext _context;

        public GetFavoriteCollectionsRepository(PolyBucketDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<GetFavoriteCollectionsResponse>> GetFavoriteCollectionsByUserIdAsync(Guid userId)
        {
            var collections = await _context.Collections
                .Where(c => c.OwnerId == userId && c.Favorite)
                .Select(c => new GetFavoriteCollectionsResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Visibility = c.Visibility,
                    Avatar = c.Avatar,
                    Favorite = c.Favorite,
                    DisplayOrder = c.DisplayOrder,
                    ModelCount = c.CollectionModels.Count,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt ?? c.CreatedAt
                })
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .AsNoTracking()
                .ToListAsync();

            return collections;
        }
    }
}
