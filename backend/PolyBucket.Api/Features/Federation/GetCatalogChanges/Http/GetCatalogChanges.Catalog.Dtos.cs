namespace PolyBucket.Api.Features.Federation.GetCatalogChanges.Http;

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
