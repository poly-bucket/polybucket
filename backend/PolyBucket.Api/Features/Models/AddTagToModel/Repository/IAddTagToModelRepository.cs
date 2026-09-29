using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Models.AddTagToModel.Domain;

namespace PolyBucket.Api.Features.Models.AddTagToModel.Repository;

public interface IAddTagToModelRepository
{
    Task<Model?> GetModelAsync(Guid modelId, CancellationToken cancellationToken);
    Task<Tag?> GetActiveTagByNameAsync(string name, CancellationToken cancellationToken);
    void AddTag(Tag tag);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
