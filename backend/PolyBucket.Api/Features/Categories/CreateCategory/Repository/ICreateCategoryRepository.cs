using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Models.AddCategoryToModel.Domain;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Categories.CreateCategory.Repository
{
    public interface ICreateCategoryRepository
    {
        Task<Category> CreateCategoryAsync(Category category);
        Task<Category?> GetCategoryByIdAsync(Guid id);
        Task<Category?> GetCategoryByNameAsync(string name);
    }
}
