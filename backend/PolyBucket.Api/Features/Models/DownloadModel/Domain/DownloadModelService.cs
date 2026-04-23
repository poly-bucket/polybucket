using System.IO.Compression;
using System.Linq;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PolyBucket.Api.Common.Storage;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.Models.DownloadModel.Repository;
using PolyBucket.Api.Common.Models.Enums;
using PolyBucket.Api.Settings;

namespace PolyBucket.Api.Features.Models.DownloadModel.Domain;

public class DownloadModelService(
    IDownloadModelRepository repository,
    IPermissionService permissionService,
    IStorageService storageService,
    IOptions<StorageSettings> storageOptions,
    ILogger<DownloadModelService> logger) : IDownloadModelService
{
    private readonly StorageSettings _storageSettings = storageOptions.Value;
    private readonly ILogger<DownloadModelService> _logger = logger;

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

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out _))
            {
                await repository.TryIncrementDownloadCountAsync(id, cancellationToken);
            }

            var needsZip = bundle.Files.Count > 1 || bundle.Previews.Count > 0;
            if (!needsZip && bundle.Files.Count == 1)
            {
                var file = bundle.Files[0];
                var objectKey = ExtractObjectKey(file.Path);
                if (string.IsNullOrEmpty(objectKey))
                {
                    _logger.LogError("Could not extract object key from path: {FilePath} for single file download: {FileName}", file.Path, file.Name);
                    return DownloadModelOutcome.Error500("Invalid file path format");
                }

                try
                {
                    var fileStream = await storageService.DownloadAsync(objectKey);
                    _logger.LogInformation("Successfully downloaded single file {FileName} for model {ModelId}", file.Name, bundle.Id);
                    return DownloadModelOutcome.OkSingle(fileStream, file.MimeType, file.Name, ownerDisposes: true);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to download single file {FileName} for model {ModelId}: {ErrorMessage}", file.Name, bundle.Id, ex.Message);
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
                    try
                    {
                        var objectKey = ExtractObjectKey(file.Path);
                        if (string.IsNullOrEmpty(objectKey))
                        {
                            _logger.LogWarning("Could not extract object key from path: {FilePath} for file: {FileName}", file.Path, file.Name);
                            failedFiles.Add(file.Name);
                            continue;
                        }

                        await using var fileStream = await storageService.DownloadAsync(objectKey);
                        if (fileStream is not { Length: > 0 })
                        {
                            _logger.LogWarning("File stream is null or empty for file: {FileName}", file.Name);
                            failedFiles.Add(file.Name);
                            continue;
                        }

                        var entry = archive.CreateEntry($"files/{file.Name}", CompressionLevel.Optimal);
                        await using var entryStream = entry.Open();
                        await fileStream.CopyToAsync(entryStream, cancellationToken);
                        await entryStream.FlushAsync(cancellationToken);
                        totalFilesAdded++;
                        _logger.LogDebug("Successfully added file {FileName} ({FileSize} bytes) to archive for model {ModelId}", file.Name, fileStream.Length, bundle.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to download file {FileName} (ID: {FileId}) for model {ModelId}: {ErrorMessage}", file.Name, file.Id, bundle.Id, ex.Message);
                        failedFiles.Add(file.Name);
                    }
                }

                foreach (var preview in bundle.Previews)
                {
                    try
                    {
                        var objectKey = ExtractObjectKey(preview.StorageKey);
                        if (string.IsNullOrEmpty(objectKey))
                        {
                            _logger.LogWarning("Could not extract object key from storage key: {StorageKey} for preview: {PreviewId}", preview.StorageKey, preview.Id);
                            failedFiles.Add($"preview_{preview.Size}");
                            continue;
                        }

                        await using var previewStream = await storageService.DownloadAsync(objectKey);
                        if (previewStream is not { Length: > 0 })
                        {
                            _logger.LogWarning("Preview stream is null or empty for preview: {PreviewId}", preview.Id);
                            failedFiles.Add($"preview_{preview.Size}");
                            continue;
                        }

                        var pFileName = $"preview_{preview.Size}_{preview.Width}x{preview.Height}.jpg";
                        var entry = archive.CreateEntry($"previews/{pFileName}", CompressionLevel.Optimal);
                        await using var entryStream = entry.Open();
                        await previewStream.CopyToAsync(entryStream, cancellationToken);
                        await entryStream.FlushAsync(cancellationToken);
                        totalFilesAdded++;
                        _logger.LogDebug("Successfully added preview {PreviewSize} ({FileSize} bytes) to archive for model {ModelId}", preview.Size, previewStream.Length, bundle.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to download preview {PreviewId} for model {ModelId}: {ErrorMessage}", preview.Id, bundle.Id, ex.Message);
                        failedFiles.Add($"preview_{preview.Size}");
                    }
                }

                if (!string.IsNullOrEmpty(bundle.ThumbnailUrl) && bundle.Previews.Count == 0)
                {
                    try
                    {
                        var objectKey = ExtractObjectKey(bundle.ThumbnailUrl);
                        if (!string.IsNullOrEmpty(objectKey))
                        {
                            await using var thumbnailStream = await storageService.DownloadAsync(objectKey);
                            if (thumbnailStream is { Length: > 0 })
                            {
                                var entry = archive.CreateEntry("thumbnail.jpg", CompressionLevel.Optimal);
                                await using var entryStream = entry.Open();
                                await thumbnailStream.CopyToAsync(entryStream, cancellationToken);
                                await entryStream.FlushAsync(cancellationToken);
                                totalFilesAdded++;
                                _logger.LogDebug("Successfully added thumbnail ({FileSize} bytes) to archive for model {ModelId}", thumbnailStream.Length, bundle.Id);
                            }
                            else
                            {
                                _logger.LogWarning("Thumbnail stream is null or empty for model {ModelId}", bundle.Id);
                                failedFiles.Add("thumbnail");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to download thumbnail for model {ModelId}: {ErrorMessage}", bundle.Id, ex.Message);
                        failedFiles.Add("thumbnail");
                    }
                }

                if (totalFilesAdded == 0)
                {
                    var entry = archive.CreateEntry("README.txt", CompressionLevel.Optimal);
                    await using var entryStream = entry.Open();
                    using var writer = new StreamWriter(entryStream);
                    await writer.WriteLineAsync($"Model: {bundle.Name}");
                    await writer.WriteLineAsync($"Download Date: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                    await writer.WriteLineAsync("Note: No files were available for download.");
                    await writer.FlushAsync(cancellationToken);
                    await entryStream.FlushAsync(cancellationToken);
                    totalFilesAdded++;
                    _logger.LogInformation("Created README.txt as no files were available for model {ModelId}", bundle.Id);
                }

                _logger.LogInformation("ZIP archive creation completed for model {ModelId}: {TotalFiles} files added, {FailedFiles} failures", bundle.Id, totalFilesAdded, failedFiles.Count);
            }

            memoryStream.Position = 0;
            if (memoryStream.Length == 0)
            {
                _logger.LogError("ZIP archive is empty after creation for model {ModelId}", bundle.Id);
                return DownloadModelOutcome.Error500("Failed to create download archive - archive is empty");
            }

            if (failedFiles.Count > 0)
            {
                _logger.LogWarning("Download completed for model {ModelId} with {FailedCount} failed files: {FailedFiles}", bundle.Id, failedFiles.Count, string.Join(", ", failedFiles));
            }
            else
            {
                _logger.LogInformation("Successfully created download archive for model {ModelId} with {FileCount} files and {PreviewCount} previews", bundle.Id, bundle.Files.Count, bundle.Previews.Count);
            }

            _logger.LogInformation("Returning ZIP file for model {ModelId}: {FileName} ({FileSize} bytes)", bundle.Id, zipFileName, memoryStream.Length);
            var zipBytes = memoryStream.ToArray();
            if (zipBytes.Length < 4
                || zipBytes[0] != 0x50
                || zipBytes[1] != 0x4B
                || zipBytes[2] != 0x03
                || zipBytes[3] != 0x04)
            {
                _logger.LogError("ZIP file validation failed for model {ModelId}: Invalid ZIP header", bundle.Id);
                return DownloadModelOutcome.Error500("Failed to create valid ZIP archive");
            }

            _logger.LogInformation("ZIP file validation passed for model {ModelId}: Header bytes: {HeaderBytes}", bundle.Id, BitConverter.ToString(zipBytes.AsSpan(0, 4).ToArray()));
            try
            {
                using var validationStream = new MemoryStream(zipBytes);
                using var validationArchive = new ZipArchive(validationStream, ZipArchiveMode.Read);
                var entryCount = validationArchive.Entries.Count;
                _logger.LogInformation("ZIP file structure validation passed for model {ModelId}: {EntryCount} entries found", bundle.Id, entryCount);
                foreach (var entry in validationArchive.Entries)
                {
                    _logger.LogDebug("ZIP entry: {EntryName}, Size: {EntrySize}, Compressed: {CompressedSize}", entry.Name, entry.Length, entry.CompressedLength);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ZIP file structure validation failed for model {ModelId}: {ErrorMessage}", bundle.Id, ex.Message);
                return DownloadModelOutcome.Error500("Failed to create valid ZIP archive structure");
            }

            return DownloadModelOutcome.OkZipFile(zipBytes, zipFileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error occurred while downloading model {ModelId}: {ErrorMessage}", id, ex.Message);
            return DownloadModelOutcome.Error500("An error occurred while downloading the model");
        }
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

    private string? ExtractObjectKey(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return null;
        }

        if (!filePath.StartsWith("http", StringComparison.Ordinal))
        {
            return filePath;
        }

        try
        {
            var uri = new Uri(filePath);
            var pathSegments = uri.AbsolutePath.Split('/');
            var bucketIndex = Array.IndexOf(pathSegments, _storageSettings.BucketName);
            if (bucketIndex >= 0 && bucketIndex + 1 < pathSegments.Length)
            {
                return string.Join("/", pathSegments.Skip(bucketIndex + 1));
            }

            _logger.LogWarning("Could not find bucket name '{BucketName}' in URL path: {FilePath}", _storageSettings.BucketName, filePath);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse URL for object key extraction: {FilePath}", filePath);
            return null;
        }
    }
}
