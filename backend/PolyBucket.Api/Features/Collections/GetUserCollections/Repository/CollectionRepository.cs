using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Collections.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.GetUserCollections.Repository
{
    public class CollectionRepository(PolyBucketDbContext context) : ICollectionRepository
    {
        private readonly PolyBucketDbContext _context = context;

        public async Task<IEnumerable<Collection>> GetCollectionsByUserIdAsync(Guid userId)
        {
            return await _context.Collections
                .Include(c => c.CollectionModels)
                .ThenInclude(cm => cm.Model)
                .Where(c => c.OwnerId == userId)
                .OrderBy(c => c.Name)
                .AsNoTracking()
                .ToListAsync();
        }
    }
} 