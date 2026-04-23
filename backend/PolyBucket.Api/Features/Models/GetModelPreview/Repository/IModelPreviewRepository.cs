using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Domain;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.GetModelPreview.Repository
{
    public interface IModelPreviewRepository
    {
        Task<ModelPreview?> GetPreviewAsync(Guid modelId, string size);
        Task<ModelPreview> CreatePreviewAsync(ModelPreview preview);
        Task<ModelPreview> UpdatePreviewAsync(ModelPreview preview);
        Task<bool> ExistsAsync(Guid modelId, string size);
    }
} 