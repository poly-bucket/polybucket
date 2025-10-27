using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.Shared.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.DeleteModel.Repository
{
    public class DeleteModelRepository : IDeleteModelRepository
    {
        private readonly PolyBucketDbContext _dbContext;

        public DeleteModelRepository(PolyBucketDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Model?> GetModelByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _dbContext.Models
                .Include(m => m.Files)
                .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        }

        public async Task DeleteModelAsync(Model model, CancellationToken cancellationToken)
        {
            _dbContext.Models.Update(model);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
} 