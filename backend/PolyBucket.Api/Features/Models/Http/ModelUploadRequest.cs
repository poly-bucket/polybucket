using Microsoft.AspNetCore.Http;

namespace PolyBucket.Api.Features.Models.Http;

public class ModelUploadRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Privacy { get; set; }
    public string? License { get; set; }
    public string? Categories { get; set; }
    public bool AIGenerated { get; set; }
    public bool WorkInProgress { get; set; }
    public bool NSFW { get; set; }
    public bool Remix { get; set; }
    public string? ThumbnailFileId { get; set; }
    public IFormFile[] Files { get; set; } = Array.Empty<IFormFile>();
} 