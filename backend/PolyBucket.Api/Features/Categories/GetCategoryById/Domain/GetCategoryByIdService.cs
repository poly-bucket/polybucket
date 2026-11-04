using PolyBucket.Api.Features.Categories.GetCategoryById.Http;
using PolyBucket.Api.Features.Categories.GetCategoryById.Repository;

namespace PolyBucket.Api.Features.Categories.GetCategoryById.Domain;

public interface IGetCategoryByIdService
{
    Task<GetCategoryByIdResponseDto?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

public class GetCategoryByIdService(IGetCategoryByIdRepository repository) : IGetCategoryByIdService
{
    public Task<GetCategoryByIdResponseDto?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        repository.GetCategoryByIdAsync(id, cancellationToken);
}
