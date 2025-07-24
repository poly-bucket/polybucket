using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.UpdateModel.Repository
{
    public class UpdateModelRepository : IUpdateModelRepository
    {
        private readonly PolyBucketDbContext _dbContext;

        public UpdateModelRepository(PolyBucketDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Model?> GetModelByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _dbContext.Models
                .Include(m => m.Files)
                .Include(m => m.Categories)
                .Include(m => m.Tags)
                .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        }

        public async Task<Model> UpdateModelAsync(Model model, CancellationToken cancellationToken)
        {
            _dbContext.Models.Update(model);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return model;
        }
    }
} 