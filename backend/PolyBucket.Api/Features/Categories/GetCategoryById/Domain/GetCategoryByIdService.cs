using PolyBucket.Api.Features.Categories.GetCategoryById.Http;
using PolyBucket.Api.Features.Categories.GetCategoryById.Repository;

namespace PolyBucket.Api.Features.Categories.GetCategoryById.Domain;

public interface IGetCategoryByIdService
{
    Task<GetCategoryByIdResponseDto?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

public class GetCategoryByIdService(IGetCategoryByIdRepository repository) : IGetCategoryByIdService
{
    public async Task<GetCategoryByIdResponseDto?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await repository.GetCategoryByIdAsync(id);
        return category == null ? null : GetCategoryByIdMapper.ToDto(category);
    }
}
