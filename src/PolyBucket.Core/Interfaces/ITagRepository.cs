using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PolyBucket.Core.Entities;

namespace PolyBucket.Core.Interfaces
{
    public interface ITagRepository
    {
        Task<Tag> GetByIdAsync(Guid id);
        Task<Tag> GetByNameAsync(string name);
        Task<Tag> GetBySlugAsync(string slug);
        Task<IEnumerable<Tag>> GetAllAsync();
        Task<IEnumerable<Tag>> SearchAsync(string query);
        Task<IEnumerable<Tag>> GetPopularTagsAsync(int count = 20);
        Task<IEnumerable<Tag>> GetTagsByModelIdAsync(Guid modelId);
        Task<Tag> AddAsync(Tag tag);
        Task<Tag> UpdateAsync(Tag tag);
        Task<bool> DeleteAsync(Guid id);
    }
} 