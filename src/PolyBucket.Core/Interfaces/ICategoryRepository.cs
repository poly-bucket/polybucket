using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PolyBucket.Core.Entities;

namespace PolyBucket.Core.Interfaces
{
    public interface ICategoryRepository
    {
        Task<Category> GetByIdAsync(Guid id);
        Task<Category> GetBySlugAsync(string slug);
        Task<IEnumerable<Category>> GetAllAsync();
        Task<IEnumerable<Category>> GetRootCategoriesAsync();
        Task<IEnumerable<Category>> GetChildCategoriesAsync(Guid parentId);
        Task<IEnumerable<Category>> GetCategoriesByModelIdAsync(Guid modelId);
        Task<Category> AddAsync(Category category);
        Task<Category> UpdateAsync(Category category);
        Task<bool> DeleteAsync(Guid id);
    }
} 