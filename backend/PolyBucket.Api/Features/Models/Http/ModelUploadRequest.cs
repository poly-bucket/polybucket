using Microsoft.AspNetCore.Http;

namespace PolyBucket.Api.Features.Models.Http;

public class ModelUploadRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public IFormFile File { get; set; } = null!;
} 