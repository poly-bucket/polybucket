namespace PolyBucket.Api.Features.Federation.GetCatalog.Http;

public class CatalogResponse
{
    public Guid InstanceId { get; set; }
    public string InstanceName { get; set; } = string.Empty;
    public int TotalModels { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<CatalogModelDto> Models { get; set; } = new();
}

public class CatalogModelDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public CatalogAuthorDto Author { get; set; } = new();
    public string License { get; set; } = string.Empty;
    public List<string> Categories { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public long FilesSize { get; set; }
    public int FileCount { get; set; }
    public int Downloads { get; set; }
    public int Likes { get; set; }
    public int Dislikes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CatalogAuthorDto
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}

public class CatalogChangesResponse
{
    public int NewModels { get; set; }
    public int UpdatedModels { get; set; }
    public int DeletedModels { get; set; }
    public List<CatalogChangeDto> Models { get; set; } = new();
}

public class CatalogChangeDto
{
    public string Id { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
    public int? Version { get; set; }
}
