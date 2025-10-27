using PolyBucket.Api.Features.Models.Shared.Domain;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Domain;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.GenerateModelPreview.Repository
{
    public interface IGenerateModelPreviewRepository
    {
        Task<ModelPreview?> GetPreviewAsync(Guid modelId, string size);
        Task<ModelPreview> CreatePreviewAsync(ModelPreview preview);
        Task<ModelPreview> UpdatePreviewAsync(ModelPreview preview);
        Task<bool> ExistsAsync(Guid modelId, string size);
    }
} 