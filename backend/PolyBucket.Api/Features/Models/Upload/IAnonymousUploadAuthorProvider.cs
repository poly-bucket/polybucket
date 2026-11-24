namespace PolyBucket.Api.Features.Models.Upload;

public interface IAnonymousUploadAuthorProvider
{
    Task<Guid> GetAuthorIdAsync(CancellationToken cancellationToken = default);
}