using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PolyBucket.Core.Entities;
using PolyBucket.Core.Models;

namespace PolyBucket.Core.Interfaces
{
    public interface IModelService
    {
        Task<Model> GetModelByIdAsync(Guid id);
        Task<IEnumerable<Model>> GetAllModelsAsync(int page = 1, int pageSize = 20);
        Task<IEnumerable<Model>> GetUserModelsAsync(Guid userId, int page = 1, int pageSize = 20);
        Task<IEnumerable<Model>> SearchModelsAsync(string query, int page = 1, int pageSize = 20);
        Task<ModelUploadResult> UploadModelAsync(ModelUploadRequest request);
        Task<ModelUploadResult> UpdateModelAsync(Guid id, ModelUpdateRequest request);
        Task<bool> DeleteModelAsync(Guid id);
        Task<bool> AddTagToModelAsync(Guid modelId, string tagName);
        Task<bool> RemoveTagFromModelAsync(Guid modelId, Guid tagId);
        Task<bool> AddCategoryToModelAsync(Guid modelId, Guid categoryId);
        Task<bool> RemoveCategoryFromModelAsync(Guid modelId, Guid categoryId);
        Task<Stream> DownloadModelFileAsync(Guid modelId);
        Task<bool> RecordModelInteractionAsync(Guid modelId, Guid userId, InteractionType type);
        Task<ModelVersionUploadResult> CreateModelVersionAsync(Guid parentModelId, ModelVersionUploadRequest request);
        Task<IEnumerable<Model>> GetModelVersionsAsync(Guid modelId);
        Task<bool> AddModelToCollectionAsync(Guid modelId, Guid collectionId);
        Task<bool> RemoveModelFromCollectionAsync(Guid modelId, Guid collectionId);
        
        // Moderation methods
        Task<IEnumerable<Model>> GetModelsAwaitingModerationAsync(int page = 1, int pageSize = 20);
        Task<bool> ApproveModelAsync(Guid modelId, Guid moderatorId);
        Task<bool> RejectModelAsync(Guid modelId, Guid moderatorId, string reason);
        Task<bool> IsUserModeratorAsync(Guid userId);
    }
} 