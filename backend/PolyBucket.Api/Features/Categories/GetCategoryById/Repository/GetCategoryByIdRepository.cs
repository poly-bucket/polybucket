using PolyBucket.Api.Features.Categories.CreateCategory.Domain;
using PolyBucket.Api.Features.Categories.CreateCategory.Repository;

namespace PolyBucket.Api.Features.Categories.GetCategoryById.Repository;

public interface IGetCategoryByIdRepository
{
    Task<CreateCategoryResponse?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

public class GetCategoryByIdRepository(ICreateCategoryRepository createCategoryRepository) : IGetCategoryByIdRepository
{
    public async Task<CreateCategoryResponse?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await createCategoryRepository.GetCategoryByIdAsync(id);
        if (category == null)
        {
            return null;
        }

        return new CreateCategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            CreatedAt = category.CreatedAt,
            CreatedById = category.CreatedById
        };
    }
}
