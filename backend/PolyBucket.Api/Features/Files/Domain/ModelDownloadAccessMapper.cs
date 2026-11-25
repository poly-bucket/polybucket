namespace PolyBucket.Api.Features.Files.Domain;

public static class ModelDownloadAccessMapper
{
    public static StreamModelFile.Domain.StreamModelFileStatus ToStreamModelFileStatus(
        ModelDownloadAccessResult access)
    {
        return access switch
        {
            ModelDownloadAccessResult.Allowed => throw new InvalidOperationException("Allowed is not an error status."),
            ModelDownloadAccessResult.Disabled => StreamModelFile.Domain.StreamModelFileStatus.Disabled,
            ModelDownloadAccessResult.SignInRequired => StreamModelFile.Domain.StreamModelFileStatus.Unauthorized,
            ModelDownloadAccessResult.Forbidden => StreamModelFile.Domain.StreamModelFileStatus.Forbidden,
            _ => StreamModelFile.Domain.StreamModelFileStatus.Forbidden
        };
    }

    public static StreamFileById.Domain.StreamFileByIdStatus ToStreamFileByIdStatus(
        ModelDownloadAccessResult access)
    {
        return access switch
        {
            ModelDownloadAccessResult.Allowed => throw new InvalidOperationException("Allowed is not an error status."),
            ModelDownloadAccessResult.Disabled => StreamFileById.Domain.StreamFileByIdStatus.Disabled,
            ModelDownloadAccessResult.SignInRequired => StreamFileById.Domain.StreamFileByIdStatus.Unauthorized,
            ModelDownloadAccessResult.Forbidden => StreamFileById.Domain.StreamFileByIdStatus.Forbidden,
            _ => StreamFileById.Domain.StreamFileByIdStatus.Forbidden
        };
    }
}