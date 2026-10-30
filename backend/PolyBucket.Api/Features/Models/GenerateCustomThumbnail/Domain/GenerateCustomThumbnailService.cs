namespace PolyBucket.Api.Features.Models.GenerateCustomThumbnail.Domain;

public class GenerateCustomThumbnailService : IGenerateCustomThumbnailService
{
    public Task<GenerateCustomThumbnailResponse> GenerateAsync(GenerateCustomThumbnailCommand command, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new GenerateCustomThumbnailResponse
        {
            ModelId = command.ModelId,
            Size = command.Size,
            PreviewUrl = string.Empty,
            Status = "queued",
            Message = "Custom thumbnail generation queued",
            IsQueued = true,
            QueuedAt = DateTime.UtcNow
        });
    }
}
