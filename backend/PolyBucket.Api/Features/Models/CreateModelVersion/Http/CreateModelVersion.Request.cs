namespace PolyBucket.Api.Features.Models.CreateModelVersion.Http;
public class ModelVersionUploadRequest
{
    public string? Name { get; set; }
    public string? Notes { get; set; }
    public string? ThumbnailFileId { get; set; }
    public IFormFile[] Files { get; set; } = Array.Empty<IFormFile>();
}