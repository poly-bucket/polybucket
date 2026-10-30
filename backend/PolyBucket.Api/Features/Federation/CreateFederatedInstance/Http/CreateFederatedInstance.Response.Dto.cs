namespace PolyBucket.Api.Features.Federation.CreateFederatedInstance.Http;

public class CreateFederatedInstanceResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
}
