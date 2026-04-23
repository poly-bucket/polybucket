using PolyBucket.Api.Common.Models;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Federation.Services
{
    public interface IFederationImportService
    {
        Task<Model> ImportModelAsync(string instanceId, string remoteModelId);
    }
}

