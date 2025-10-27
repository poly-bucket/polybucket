using PolyBucket.Api.Features.Models.Shared.Domain;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.GetModelByUserId.Repository
{
    public interface IGetModelByUserIdRepository
    {
        Task<(IEnumerable<Model> Models, int TotalCount)> GetModelsByUserIdAsync(Guid userId, int page, int take, bool includePrivate, bool includeDeleted, CancellationToken cancellationToken);
    }
} 