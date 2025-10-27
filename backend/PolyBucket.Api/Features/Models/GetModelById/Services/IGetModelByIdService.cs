using System;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Models.Shared.Domain;

namespace PolyBucket.Api.Features.Models.GetModelById.Services
{
    public interface IGetModelByIdService
    {
        Task<Model?> GetModelByIdAsync(Guid id);
    }
}
