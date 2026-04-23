using System;
using System.Threading.Tasks;
using PolyBucket.Api.Common.Models;

namespace PolyBucket.Api.Features.Models.GetModelById.Repository
{
    public interface IGetModelByIdRepository
    {
        Task<Model?> GetModelByIdAsync(Guid id);
    }
}
