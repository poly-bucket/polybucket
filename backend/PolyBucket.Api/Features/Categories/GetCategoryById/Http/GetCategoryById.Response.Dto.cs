using PolyBucket.Database.Entities.Models;

namespace PolyBucket.Api.Features.Categories.GetCategoryById.Http;

public class GetCategoryByIdResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid CreatedById { get; set; }
}

public static class GetCategoryByIdMapper
{
    public static GetCategoryByIdResponseDto ToDto(Category category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Description = category.Description,
        Icon = category.Icon,
        Color = category.Color,
        CreatedAt = category.CreatedAt,
        CreatedById = category.CreatedById
    };
}
