using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Domain;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.GenerateModelPreview.Repository
{
    public class GenerateModelPreviewRepository(PolyBucketDbContext db) : IGenerateModelPreviewRepository
    {
        private readonly PolyBucketDbContext _db = db;

        public async Task<ModelPreview?> GetPreviewAsync(Guid modelId, string size)
        {
            return await _db.ModelPreviews
                .FirstOrDefaultAsync(p => p.ModelId == modelId && p.Size == size);
        }

        public async Task<ModelPreview> CreatePreviewAsync(ModelPreview preview)
        {
            _db.ModelPreviews.Add(preview);
            await _db.SaveChangesAsync();
            return preview;
        }

        public async Task<ModelPreview> UpdatePreviewAsync(ModelPreview preview)
        {
            _db.ModelPreviews.Update(preview);
            await _db.SaveChangesAsync();
            return preview;
        }

        public async Task<bool> ExistsAsync(Guid modelId, string size)
        {
            return await _db.ModelPreviews
                .AnyAsync(p => p.ModelId == modelId && p.Size == size);
        }
    }
} 