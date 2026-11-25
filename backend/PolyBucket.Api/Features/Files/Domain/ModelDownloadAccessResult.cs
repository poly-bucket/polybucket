namespace PolyBucket.Api.Features.Files.Domain;

public enum ModelDownloadAccessResult
{
    Allowed,
    Disabled,
    SignInRequired,
    Forbidden
}