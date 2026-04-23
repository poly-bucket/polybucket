using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Models.CreateModelVersion.Domain;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.CreateModelVersion.Repository
{
    public class CreateModelVersionRepository : ICreateModelVersionRepository
    {
        private readonly PolyBucketDbContext _dbContext;

        public CreateModelVersionRepository(PolyBucketDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Model?> GetModelByIdAsync(Guid modelId, CancellationToken cancellationToken)
        {
            return await _dbContext.Models
                .Include(m => m.Versions)
                .FirstOrDefaultAsync(m => m.Id == modelId, cancellationToken);
        }

        public async Task<ModelVersion> CreateModelVersionAsync(ModelVersion modelVersion, CancellationToken cancellationToken)
        {
            _dbContext.ModelVersions.Add(modelVersion);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return modelVersion;
        }

        public async Task<int> GetNextVersionNumberAsync(Guid modelId, CancellationToken cancellationToken)
        {
            var maxVersionNumber = await _dbContext.ModelVersions
                .Where(v => v.ModelId == modelId)
                .MaxAsync(v => (int?)v.VersionNumber, cancellationToken);

            return (maxVersionNumber ?? 0) + 1;
        }
    }
} 