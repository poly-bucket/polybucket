using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Models.Shared.Domain;

namespace PolyBucket.Api.Features.Models.GetModels.Repository
{
    public interface IGetModelsRepository
    {
        Task<(IEnumerable<Model> Models, int TotalCount)> GetModelsAsync(int page, int take, string? sortBy);
    }
}
