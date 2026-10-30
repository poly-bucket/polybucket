using System.Security.Claims;
using PolyBucket.Api.Features.Files.Domain;
using PolyBucket.Api.Features.Files.StreamFileById.Repository;

namespace PolyBucket.Api.Features.Files.StreamFileById.Domain;

public interface IStreamFileByIdService
{
    Task<StreamFileByIdResult> StreamAsync(Guid fileId, ClaimsPrincipal user, CancellationToken cancellationToken = default);
}

public class StreamFileByIdService(
    IStreamFileByIdRepository repository,
    IModelFileAccessEvaluator accessEvaluator) : IStreamFileByIdService
{
    public async Task<StreamFileByIdResult> StreamAsync(Guid fileId, ClaimsPrincipal user, CancellationToken cancellationToken = default)
    {
        var file = await repository.GetFileWithModelAsync(fileId, cancellationToken);
        if (file?.Model == null)
        {
            return new StreamFileByIdResult(StreamFileByIdStatus.NotFound, null, null, null);
        }

        if (!await accessEvaluator.CanUserAccessModelAsync(file.Model, user, cancellationToken))
        {
            return new StreamFileByIdResult(StreamFileByIdStatus.Forbidden, null, null, null);
        }

        var stream = await repository.DownloadAsync(file.Path, cancellationToken);
        if (stream == null)
        {
            return new StreamFileByIdResult(StreamFileByIdStatus.StorageMissing, null, null, null);
        }

        return new StreamFileByIdResult(
            StreamFileByIdStatus.Success,
            stream,
            file.Name,
            file.MimeType ?? "application/octet-stream");
    }
}
