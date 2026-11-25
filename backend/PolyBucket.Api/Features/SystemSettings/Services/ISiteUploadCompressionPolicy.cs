namespace PolyBucket.Api.Features.SystemSettings.Services;

public interface ISiteUploadCompressionPolicy
{
    Task<bool> ShouldCompressAsync(string fileName, CancellationToken cancellationToken = default);
}