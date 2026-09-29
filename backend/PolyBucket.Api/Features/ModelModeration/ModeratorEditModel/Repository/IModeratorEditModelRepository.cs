using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Models.AddCategoryToModel.Domain;
using PolyBucket.Api.Features.Models.AddTagToModel.Domain;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ModelModeration.ModeratorEditModel.Repository;

public interface IModeratorEditModelRepository
{
    Task<Model?> GetModelForEditAsync(Guid modelId, CancellationToken cancellationToken = default);

    Task<Tag?> FindTagByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<Category?> FindCategoryByNameAsync(string name, CancellationToken cancellationToken = default);

    void AddTag(Tag tag);

    void AddCategory(Category category);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
