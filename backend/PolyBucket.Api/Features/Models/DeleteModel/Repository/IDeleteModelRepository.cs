using PolyBucket.Api.Features.Models.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.DeleteModel.Repository
{
    public interface IDeleteModelRepository
    {
        Task<Model?> GetModelByIdAsync(Guid id, CancellationToken cancellationToken);
        Task DeleteModelAsync(Model model, CancellationToken cancellationToken);
    }
} 