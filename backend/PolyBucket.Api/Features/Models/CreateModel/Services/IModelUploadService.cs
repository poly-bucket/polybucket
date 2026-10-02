using PolyBucket.Api.Common.Models;

namespace PolyBucket.Api.Features.Models.CreateModel.Services;

public interface IModelUploadService
{
    Task<Model> ProcessModelUploadAsync(Model model, CancellationToken cancellationToken = default);
}
