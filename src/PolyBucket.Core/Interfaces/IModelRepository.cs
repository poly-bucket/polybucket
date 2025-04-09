using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PolyBucket.Core.Entities;
using PolyBucket.Core.Enums;

namespace PolyBucket.Core.Interfaces
{
    public interface IModelRepository
    {
        Task<Model> GetByIdAsync(Guid id);
        Task<IEnumerable<Model>> GetAllAsync(int skip = 0, int take = 20);
        Task<IEnumerable<Model>> GetByUserIdAsync(Guid userId, int skip = 0, int take = 20);
        Task<IEnumerable<Model>> GetByCollectionIdAsync(Guid collectionId, int skip = 0, int take = 20);
        Task<IEnumerable<Model>> GetByTagIdAsync(Guid tagId, int skip = 0, int take = 20);
        Task<IEnumerable<Model>> GetByCategoryIdAsync(Guid categoryId, int skip = 0, int take = 20);
        Task<IEnumerable<Model>> SearchAsync(string query, int skip = 0, int take = 20);
        Task<Model> AddAsync(Model model);
        Task<Model> UpdateAsync(Model model);
        Task<bool> DeleteAsync(Guid id);
        Task<int> GetTotalCountAsync();
        Task<int> GetUserModelCountAsync(Guid userId);
        Task<int> IncrementDownloadCountAsync(Guid id);
        Task<int> IncrementViewCountAsync(Guid id);
        Task<IEnumerable<Model>> GetModelVersionsAsync(Guid parentModelId);
        Task<bool> AddTagToModelAsync(Guid modelId, Guid tagId);
        Task<bool> RemoveTagFromModelAsync(Guid modelId, Guid tagId);
        Task<bool> AddCategoryToModelAsync(Guid modelId, Guid categoryId);
        Task<bool> RemoveCategoryFromModelAsync(Guid modelId, Guid categoryId);
        
        // Moderation methods
        Task<IEnumerable<Model>> GetAwaitingModerationAsync(int skip = 0, int take = 20);
        Task<bool> UpdateModerationStatusAsync(Guid id, ModerationStatus status, Guid moderatorId, string reason = null);
    }
} 