namespace PolyBucket.Api.Features.Files.StreamFileById.Domain;

public enum StreamFileByIdStatus
{
    Success,
    NotFound,
    Forbidden,
    StorageMissing,
    Error
}

public sealed record StreamFileByIdResult(
    StreamFileByIdStatus Status,
    Stream? Content,
    string? FileName,
    string? MimeType);
