using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Models.AddCategoryToModel.Domain;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Categories.UpdateCategory.Repository
{
    public interface IUpdateCategoryRepository
    {
        Task<Category?> GetCategoryByIdAsync(Guid id);
        Task<Category?> GetCategoryByNameAsync(string name);
        Task<Category> UpdateCategoryAsync(Guid id, string name, Guid updatedById);
    }
}
