using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PolyBucket.Core.Entities;

namespace PolyBucket.Core.Interfaces
{
    public interface ICollectionRepository
    {
        Task<Collection> GetByIdAsync(Guid id);
        Task<IEnumerable<Collection>> GetAllAsync(int skip = 0, int take = 20);
        Task<IEnumerable<Collection>> GetByUserIdAsync(Guid userId, int skip = 0, int take = 20);
        Task<IEnumerable<Collection>> GetPublicCollectionsAsync(int skip = 0, int take = 20);
        Task<IEnumerable<Collection>> SearchAsync(string query, int skip = 0, int take = 20);
        Task<Collection> AddAsync(Collection collection);
        Task<Collection> UpdateAsync(Collection collection);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> AddModelToCollectionAsync(Guid collectionId, Guid modelId);
        Task<bool> RemoveModelFromCollectionAsync(Guid collectionId, Guid modelId);
        Task<int> GetTotalCountAsync();
        Task<int> GetUserCollectionCountAsync(Guid userId);
    }
} 