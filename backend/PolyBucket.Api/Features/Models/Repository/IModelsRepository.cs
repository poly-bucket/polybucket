using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Models.Domain;

namespace PolyBucket.Api.Features.Models.Repository
{
    public interface IModelsRepository
    {
        Task<(IEnumerable<Model> Models, int TotalCount)> GetModelsAsync(int page, int take);
        Task<Model> GetModelByIdAsync(Guid id);
    }
} 