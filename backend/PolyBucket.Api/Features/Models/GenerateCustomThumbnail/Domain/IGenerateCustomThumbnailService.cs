namespace PolyBucket.Api.Features.Models.GenerateCustomThumbnail.Domain;

public interface IGenerateCustomThumbnailService
{
    Task<GenerateCustomThumbnailResponse> GenerateAsync(GenerateCustomThumbnailCommand command, CancellationToken cancellationToken = default);
}
