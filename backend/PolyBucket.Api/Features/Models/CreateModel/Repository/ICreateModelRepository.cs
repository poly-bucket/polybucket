using PolyBucket.Api.Features.Models.Shared.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.CreateModel.Repository
{
    public interface ICreateModelRepository
    {
        Task<Model> CreateModelAsync(Model model, CancellationToken cancellationToken);
    }
} 