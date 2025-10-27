using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.Shared.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.CreateModel.Repository
{
    public class CreateModelRepository : ICreateModelRepository
    {
        private readonly PolyBucketDbContext _dbContext;

        public CreateModelRepository(PolyBucketDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Model> CreateModelAsync(Model model, CancellationToken cancellationToken)
        {
            _dbContext.Models.Add(model);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return model;
        }
    }
} 