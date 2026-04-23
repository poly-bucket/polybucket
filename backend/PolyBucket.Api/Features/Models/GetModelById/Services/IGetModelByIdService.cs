using System;
using System.Threading.Tasks;
using PolyBucket.Api.Common.Models;

namespace PolyBucket.Api.Features.Models.GetModelById.Services
{
    public interface IGetModelByIdService
    {
        Task<Model?> GetModelByIdAsync(Guid id);
    }
}
