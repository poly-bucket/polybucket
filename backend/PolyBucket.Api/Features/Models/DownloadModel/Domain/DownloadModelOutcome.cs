using System.IO;
using PolyBucket.Api.Common.Models.Enums;

namespace PolyBucket.Api.Features.Models.DownloadModel.Domain;

public enum DownloadModelOutcomeKind
{
    NotFound,
    Forbid,
    Error,
    OkSingleFile,
    OkZip
}

public sealed class DownloadModelOutcome
{
    public DownloadModelOutcomeKind Kind { get; init; }
    public string? Message { get; init; }
    public Stream? FileStream { get; init; }
    public string? FileContentType { get; init; }
    public string? FileName { get; init; }
    public byte[]? ZipBytes { get; init; }
    public string? ZipFileName { get; init; }
    public bool FileStreamOwnerDisposes { get; init; }

    public static DownloadModelOutcome NotFound() =>
        new() { Kind = DownloadModelOutcomeKind.NotFound, Message = "Model not found" };

    public static DownloadModelOutcome Forbid() =>
        new() { Kind = DownloadModelOutcomeKind.Forbid, Message = "You do not have permission to download this model" };

    public static DownloadModelOutcome Error500(string? message) =>
        new() { Kind = DownloadModelOutcomeKind.Error, Message = message ?? "An error occurred" };

    public static DownloadModelOutcome OkSingle(
        Stream stream,
        string contentType,
        string fileName,
        bool ownerDisposes) =>
        new()
        {
            Kind = DownloadModelOutcomeKind.OkSingleFile,
            FileStream = stream,
            FileContentType = contentType,
            FileName = fileName,
            FileStreamOwnerDisposes = ownerDisposes
        };

    public static DownloadModelOutcome OkZipFile(byte[] zipBytes, string zipFileName) =>
        new() { Kind = DownloadModelOutcomeKind.OkZip, ZipBytes = zipBytes, ZipFileName = zipFileName };
}

public sealed class DownloadModelBundle
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? ThumbnailUrl { get; init; }
    public PrivacySettings Privacy { get; init; }
    public required Guid AuthorId { get; init; }
    public int Downloads { get; set; }
    public List<DownloadModelFileItem> Files { get; init; } = new();
    public List<DownloadModelPreviewItem> Previews { get; init; } = new();
}

public sealed class DownloadModelFileItem
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Path { get; init; }
    public required string MimeType { get; init; }
}

public sealed class DownloadModelPreviewItem
{
    public required Guid Id { get; init; }
    public required string Size { get; init; }
    public required string StorageKey { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
}
