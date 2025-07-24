using PolyBucket.Api.Features.Models.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.UpdateModel.Repository
{
    public interface IUpdateModelRepository
    {
        Task<Model?> GetModelByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Model> UpdateModelAsync(Model model, CancellationToken cancellationToken);
    }
} 