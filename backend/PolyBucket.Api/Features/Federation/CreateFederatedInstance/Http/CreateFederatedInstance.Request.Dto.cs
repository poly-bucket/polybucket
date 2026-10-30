namespace PolyBucket.Api.Features.Federation.CreateFederatedInstance.Http;

public class CreateFederatedInstanceRequest
{
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PublicKey { get; set; }
    public string? SharedSecret { get; set; }
    public bool? IsEnabled { get; set; }
}
