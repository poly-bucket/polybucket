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

        public async Task<(IEnumerable<Collection> Collections, int TotalCount)> GetCollectionsByUserIdAsync(Guid userId, int page, int pageSize, string? searchQuery)
        {
            var query = _context.Collections
                .Include(c => c.CollectionModels)
                .ThenInclude(cm => cm.Model)
                .Where(c => c.OwnerId == userId);

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                query = query.Where(c => c.Name.Contains(searchQuery) || 
                                        (c.Description != null && c.Description.Contains(searchQuery)));
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var collections = await query
                .OrderBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return (collections, totalCount);
        }
    }
} 