using System.IO.Compression;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common.Models.Enums;
using PolyBucket.Api.Common.Storage;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.Models.Common;
using PolyBucket.Api.Features.Models.DownloadModel.Repository;
using PolyBucket.Api.Features.Models.RecordModelDownload.Domain;

namespace PolyBucket.Api.Features.Models.DownloadModel.Domain;

public class DownloadModelService(
    IDownloadModelRepository repository,
    IPermissionService permissionService,
    IStorageService storageService,
    IStorageObjectKeyResolver objectKeyResolver,
    IHttpContextAccessor httpContextAccessor,
    IModelDownloadCounter downloadCounter,
    ILogger<DownloadModelService> logger) : IDownloadModelService
{
    public async Task<DownloadModelOutcome> DownloadAsync(
        Guid id,
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var bundle = await repository.GetBundleForDownloadAsync(id, cancellationToken);
            if (bundle == null)
            {
                return DownloadModelOutcome.NotFound();
            }

            if (!await CanUserAccessModelAsync(user, bundle))
            {
                return DownloadModelOutcome.Forbid();
            }

            var needsZip = bundle.Files.Count > 1 || bundle.Previews.Count > 0;
            if (!needsZip && bundle.Files.Count == 1)
            {
                var file = bundle.Files[0];
                var objectKey = objectKeyResolver.Resolve(file.Path);
                if (string.IsNullOrEmpty(objectKey))
                {
                    logger.LogError(
                        "Could not extract object key from path: {FilePath} for single file download: {FileName}",
                        file.Path,
                        file.Name);
                    return DownloadModelOutcome.Error500("Invalid file path format");
                }

                try
                {
                    var fileStream = await storageService.DownloadAsync(objectKey, cancellationToken);
                    logger.LogInformation("Successfully downloaded single file {FileName} for model {ModelId}", file.Name, bundle.Id);
                    var (downloads, counted) = await RecordDownloadAsync(id, user, cancellationToken);
                    return DownloadModelOutcome.OkSingle(
                        fileStream,
                        file.MimeType,
                        file.Name,
                        ownerDisposes: true,
                        downloads,
                        counted);
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "Failed to download single file {FileName} for model {ModelId}: {ErrorMessage}",
                        file.Name,
                        bundle.Id,
                        ex.Message);
                    return DownloadModelOutcome.Error500("Failed to download the file");
                }
            }

            var zipFileName = $"{bundle.Name.Replace(" ", "_")}_{DateTime.UtcNow:yyyyMMdd}.zip";
            var failedFiles = new List<string>();
            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                var totalFilesAdded = 0;
                foreach (var file in bundle.Files)
                {
                    var added = await TryAddStoredObjectToZipAsync(
                        archive,
                        $"files/{file.Name}",
                        file.Path,
                        file.Name,
                        bundle.Id,
                        failedFiles,
                        cancellationToken);
                    if (added)
                    {
                        totalFilesAdded++;
                    }
                }

                foreach (var preview in bundle.Previews)
                {
                    var pFileName = $"preview_{preview.Size}_{preview.Width}x{preview.Height}.jpg";
                    var added = await TryAddStoredObjectToZipAsync(
                        archive,
                        $"previews/{pFileName}",
                        preview.StorageKey,
                        $"preview_{preview.Size}",
                        bundle.Id,
                        failedFiles,
                        cancellationToken);
                    if (added)
                    {
                        totalFilesAdded++;
                    }
                }

                if (!string.IsNullOrEmpty(bundle.ThumbnailUrl) && bundle.Previews.Count == 0)
                {
                    var added = await TryAddStoredObjectToZipAsync(
                        archive,
                        "thumbnail.jpg",
                        bundle.ThumbnailUrl,
                        "thumbnail",
                        bundle.Id,
                        failedFiles,
                        cancellationToken);
                    if (added)
                    {
                        totalFilesAdded++;
                    }
                }

                if (totalFilesAdded == 0)
                {
                    var entry = archive.CreateEntry("README.txt", CompressionLevel.Optimal);
                    await using var entryStream = entry.Open();
                    await using var writer = new StreamWriter(entryStream);
                    await writer.WriteLineAsync($"Model: {bundle.Name}");
                    await writer.WriteLineAsync($"Download Date: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                    await writer.WriteLineAsync("Note: No files were available for download.");
                    await writer.FlushAsync(cancellationToken);
                    await entryStream.FlushAsync(cancellationToken);
                    totalFilesAdded++;
                    logger.LogInformation("Created README.txt as no files were available for model {ModelId}", bundle.Id);
                }

                logger.LogInformation(
                    "ZIP archive creation completed for model {ModelId}: {TotalFiles} files added, {FailedFiles} failures",
                    bundle.Id,
                    totalFilesAdded,
                    failedFiles.Count);
            }

            memoryStream.Position = 0;
            if (memoryStream.Length == 0)
            {
                logger.LogError("ZIP archive is empty after creation for model {ModelId}", bundle.Id);
                return DownloadModelOutcome.Error500("Failed to create download archive - archive is empty");
            }

            if (failedFiles.Count > 0)
            {
                logger.LogWarning(
                    "Download completed for model {ModelId} with {FailedCount} failed files: {FailedFiles}",
                    bundle.Id,
                    failedFiles.Count,
                    string.Join(", ", failedFiles));
            }
            else
            {
                logger.LogInformation(
                    "Successfully created download archive for model {ModelId} with {FileCount} files and {PreviewCount} previews",
                    bundle.Id,
                    bundle.Files.Count,
                    bundle.Previews.Count);
            }

            logger.LogInformation(
                "Returning ZIP file for model {ModelId}: {FileName} ({FileSize} bytes)",
                bundle.Id,
                zipFileName,
                memoryStream.Length);
            var zipBytes = memoryStream.ToArray();
            if (zipBytes.Length < 4
                || zipBytes[0] != 0x50
                || zipBytes[1] != 0x4B
                || zipBytes[2] != 0x03
                || zipBytes[3] != 0x04)
            {
                logger.LogError("ZIP file validation failed for model {ModelId}: Invalid ZIP header", bundle.Id);
                return DownloadModelOutcome.Error500("Failed to create valid ZIP archive");
            }

            logger.LogInformation(
                "ZIP file validation passed for model {ModelId}: Header bytes: {HeaderBytes}",
                bundle.Id,
                BitConverter.ToString(zipBytes.AsSpan(0, 4).ToArray()));
            try
            {
                using var validationStream = new MemoryStream(zipBytes);
                using var validationArchive = new ZipArchive(validationStream, ZipArchiveMode.Read);
                var entryCount = validationArchive.Entries.Count;
                logger.LogInformation(
                    "ZIP file structure validation passed for model {ModelId}: {EntryCount} entries found",
                    bundle.Id,
                    entryCount);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ZIP file structure validation failed for model {ModelId}: {ErrorMessage}", bundle.Id, ex.Message);
                return DownloadModelOutcome.Error500("Failed to create valid ZIP archive structure");
            }

            var (zipDownloads, zipCounted) = await RecordDownloadAsync(id, user, cancellationToken);
            return DownloadModelOutcome.OkZipFile(zipBytes, zipFileName, zipDownloads, zipCounted);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Critical error occurred while downloading model {ModelId}: {ErrorMessage}", id, ex.Message);
            return DownloadModelOutcome.Error500("An error occurred while downloading the model");
        }
    }

    private async Task<bool> TryAddStoredObjectToZipAsync(
        ZipArchive archive,
        string entryName,
        string storedPath,
        string logLabel,
        Guid modelId,
        List<string> failedFiles,
        CancellationToken cancellationToken)
    {
        try
        {
            var objectKey = objectKeyResolver.Resolve(storedPath);
            if (string.IsNullOrEmpty(objectKey))
            {
                logger.LogWarning(
                    "Could not extract object key from path: {StoredPath} for item: {LogLabel}",
                    storedPath,
                    logLabel);
                failedFiles.Add(logLabel);
                return false;
            }

            await using var fileStream = await storageService.DownloadAsync(objectKey, cancellationToken);
            if (fileStream is not { Length: > 0 })
            {
                logger.LogWarning("File stream is null or empty for item: {LogLabel}", logLabel);
                failedFiles.Add(logLabel);
                return false;
            }

            var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
            await using var entryStream = entry.Open();
            await fileStream.CopyToAsync(entryStream, cancellationToken);
            await entryStream.FlushAsync(cancellationToken);
            logger.LogDebug(
                "Successfully added {LogLabel} ({FileSize} bytes) to archive for model {ModelId}",
                logLabel,
                fileStream.Length,
                modelId);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to download item {LogLabel} for model {ModelId}: {ErrorMessage}", logLabel, modelId, ex.Message);
            failedFiles.Add(logLabel);
            return false;
        }
    }

    private async Task<(int Downloads, bool Counted)> RecordDownloadAsync(
        Guid modelId,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return (0, false);
        }

        var viewerKey = ModelEngagementViewerKey.Build(httpContext, user);
        return await downloadCounter.TryRecordDownloadAsync(modelId, viewerKey, cancellationToken);
    }

    private async Task<bool> CanUserAccessModelAsync(ClaimsPrincipal user, DownloadModelBundle model)
    {
        if (model.Privacy == PrivacySettings.Public)
        {
            return true;
        }

        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
        {
            return false;
        }

        if (model.AuthorId == currentUserId)
        {
            return true;
        }

        var isAdmin = await permissionService.IsAdminAsync(currentUserId);
        var userRole = await permissionService.GetUserRoleAsync(currentUserId);
        var isModerator = userRole?.Name.Equals("Moderator", StringComparison.OrdinalIgnoreCase) == true;
        if (isAdmin || isModerator)
        {
            return true;
        }

        if (model.Privacy == PrivacySettings.Private)
        {
            return false;
        }

        return model.Privacy == PrivacySettings.Unlisted;
    }
}
