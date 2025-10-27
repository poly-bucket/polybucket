using PolyBucket.Api.Features.Models.Shared.Domain;
using PolyBucket.Api.Features.Models.AddCategoryToModel.Domain;
using PolyBucket.Api.Features.Categories.GetCategories.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Categories.GetCategories.Repository
{
    public interface IGetCategoriesRepository
    {
        Task<GetCategoriesResponse> GetCategoriesAsync(GetCategoriesQuery query);
    }
}
