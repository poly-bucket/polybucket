using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Models.CreateModelVersion.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.UpdateModelVersion.Repository
{
    public class UpdateModelVersionRepository : IUpdateModelVersionRepository
    {
        private readonly PolyBucketDbContext _dbContext;

        public UpdateModelVersionRepository(PolyBucketDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ModelVersion?> GetModelVersionAsync(Guid modelId, Guid versionId, CancellationToken cancellationToken)
        {
            return await _dbContext.ModelVersions
                .Include(v => v.Model)
                .Include(v => v.Files)
                .FirstOrDefaultAsync(v => v.ModelId == modelId && v.Id == versionId, cancellationToken);
        }

        public async Task<ModelVersion> UpdateModelVersionAsync(ModelVersion modelVersion, CancellationToken cancellationToken)
        {
            _dbContext.ModelVersions.Update(modelVersion);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return modelVersion;
        }
    }
} 