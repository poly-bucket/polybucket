using System;

namespace PolyBucket.Api.Features.ModelModeration.Domain;

public class ModelModerationQueueItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FileFormat { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public ModelModerationStatus Status { get; set; }
}
