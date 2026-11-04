using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.SystemSettings.DeleteAllFileSettings.Domain
{
    public interface IDeleteAllFileSettingsService
    {
        Task<DeleteAllFileSettingsResponse> DeleteAllFileSettingsAsync(
            DeleteAllFileSettingsRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken);
    }
}
