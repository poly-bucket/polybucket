using PolyBucket.Api.Features.Models.Domain;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Categories.DeleteCategory.Repository
{
    public interface IDeleteCategoryRepository
    {
        Task<Category?> GetCategoryByIdAsync(Guid id);
        Task<bool> IsCategoryInUseAsync(Guid id);
        Task<Category> DeleteCategoryAsync(Guid id, Guid deletedById);
    }
}
