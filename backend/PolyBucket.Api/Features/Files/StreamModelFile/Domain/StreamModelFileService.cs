using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using PolyBucket.Api.Features.Files.Domain;
using PolyBucket.Api.Features.Files.StreamModelFile.Repository;

namespace PolyBucket.Api.Features.Files.StreamModelFile.Domain;

public interface IStreamModelFileService
{
    Task<StreamModelFileResult> StreamAsync(
        Guid modelId,
        string fileName,
        bool recordDownload,
        HttpContext httpContext,
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default);
}

public class StreamModelFileService(
    IStreamModelFileRepository repository,
    IModelFileAccessEvaluator accessEvaluator,
    TimeProvider timeProvider) : IStreamModelFileService
{
    private static readonly TimeSpan DedupWindow = TimeSpan.FromHours(24);

    public async Task<StreamModelFileResult> StreamAsync(
        Guid modelId,
        string fileName,
        bool recordDownload,
        HttpContext httpContext,
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        var decodedFileName = Uri.UnescapeDataString(fileName);
        var model = await repository.GetModelWithFilesAsync(modelId, cancellationToken);
        if (model == null)
        {
            return new StreamModelFileResult(StreamModelFileStatus.ModelNotFound, null, null, null, 0, false, null);
        }

        if (!await accessEvaluator.CanUserAccessModelAsync(model, user, cancellationToken))
        {
            return new StreamModelFileResult(StreamModelFileStatus.Forbidden, null, null, null, 0, false, null);
        }

        var file = model.Files.FirstOrDefault(f =>
            string.Equals(f.Name, decodedFileName, StringComparison.OrdinalIgnoreCase));
        if (file == null)
        {
            var available = string.Join(", ", model.Files.Select(f => f.Name));
            return new StreamModelFileResult(
                StreamModelFileStatus.FileNotFound,
                null,
                null,
                null,
                0,
                false,
                available);
        }

        var objectKey = repository.ResolveObjectKey(file.Path);
        if (string.IsNullOrEmpty(objectKey))
        {
            return new StreamModelFileResult(StreamModelFileStatus.InvalidPath, null, null, null, 0, false, null);
        }

        var stream = await repository.DownloadAsync(objectKey, cancellationToken);
        if (stream == null)
        {
            return new StreamModelFileResult(StreamModelFileStatus.StorageMissing, null, null, null, 0, false, null);
        }

        var downloads = 0;
        var counted = false;
        if (recordDownload)
        {
            var viewerKey = StreamModelFileViewerKey.Build(httpContext, user);
            var nowUtc = timeProvider.GetUtcNow().UtcDateTime;
            (downloads, counted) = await repository.TryRecordDownloadAsync(
                modelId,
                viewerKey,
                nowUtc,
                nowUtc - DedupWindow,
                cancellationToken);
        }

        return new StreamModelFileResult(
            StreamModelFileStatus.Success,
            stream,
            file.Name,
            file.MimeType ?? "application/octet-stream",
            downloads,
            counted,
            null);
    }
}
