using System;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Models.Shared.Domain;

namespace PolyBucket.Api.Features.Models.GetModelById.Repository
{
    public interface IGetModelByIdRepository
    {
        Task<Model?> GetModelByIdAsync(Guid id);
    }
}
