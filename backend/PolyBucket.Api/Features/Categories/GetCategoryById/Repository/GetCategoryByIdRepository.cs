using PolyBucket.Api.Features.Categories.CreateCategory.Repository;
using PolyBucket.Api.Features.Categories.GetCategoryById.Http;
using PolyBucket.Database.Entities.Models;

namespace PolyBucket.Api.Features.Categories.GetCategoryById.Repository;

public interface IGetCategoryByIdRepository
{
    Task<GetCategoryByIdResponseDto?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

public class GetCategoryByIdRepository(ICreateCategoryRepository createCategoryRepository) : IGetCategoryByIdRepository
{
    public async Task<GetCategoryByIdResponseDto?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await createCategoryRepository.GetCategoryByIdAsync(id);
        return category == null ? null : GetCategoryByIdMapper.ToDto(category);
    }
}
