using System;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Models.Domain;

namespace PolyBucket.Api.Features.Models.Domain
{
    public interface IModelService
    {
        Task<Model?> GetModelByIdAsync(Guid id);
    }
} 