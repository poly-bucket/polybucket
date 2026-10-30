using PolyBucket.Api.Features.Categories.CreateCategory.Domain;

namespace PolyBucket.Api.Features.Categories.GetCategoryById.Http;

public class GetCategoryByIdResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid CreatedById { get; set; }
}

public static class GetCategoryByIdMapper
{
    public static GetCategoryByIdResponseDto ToDto(CreateCategoryResponse response) => new()
    {
        Id = response.Id,
        Name = response.Name,
        CreatedAt = response.CreatedAt,
        CreatedById = response.CreatedById
    };
}
