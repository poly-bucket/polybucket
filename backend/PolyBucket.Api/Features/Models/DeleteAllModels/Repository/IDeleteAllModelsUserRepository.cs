using System;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Database.Entities.Models;

namespace PolyBucket.Api.Features.Models.DeleteAllModels.Repository;

public interface IDeleteAllModelsUserRepository
{
    Task<User?> GetByIdAsNoTrackingAsync(Guid id, CancellationToken cancellationToken = default);
}
