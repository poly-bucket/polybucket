namespace PolyBucket.Api.Features.Files.StreamModelFile.Domain;

public enum StreamModelFileStatus
{
    Success,
    ModelNotFound,
    FileNotFound,
    Forbidden,
    StorageMissing,
    InvalidPath,
    Error
}

public sealed record StreamModelFileResult(
    StreamModelFileStatus Status,
    Stream? Content,
    string? FileName,
    string? MimeType,
    int Downloads,
    bool DownloadCounted,
    string? AvailableFilesMessage);
