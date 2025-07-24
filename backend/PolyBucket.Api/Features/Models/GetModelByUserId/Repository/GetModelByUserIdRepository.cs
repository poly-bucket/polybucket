using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.Domain;
using PolyBucket.Api.Features.Models.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.GetModelByUserId.Repository
{
    public class GetModelByUserIdRepository : IGetModelByUserIdRepository
    {
        private readonly PolyBucketDbContext _dbContext;

        public GetModelByUserIdRepository(PolyBucketDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<(IEnumerable<Model> Models, int TotalCount)> GetModelsByUserIdAsync(Guid userId, int page, int take, bool includePrivate, bool includeDeleted, CancellationToken cancellationToken)
        {
            var query = _dbContext.Models
                .Include(m => m.Files)
                .Include(m => m.Categories)
                .Include(m => m.Tags)
                .Where(m => m.AuthorId == userId);

            if (!includeDeleted)
            {
                query = query.Where(m => m.DeletedAt == null);
            }

            if (!includePrivate)
            {
                query = query.Where(m => m.Privacy == PrivacySettings.Public);
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var models = await query
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * take)
                .Take(take)
                .ToListAsync(cancellationToken);

            return (models, totalCount);
        }
    }
} 