using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Users.UnbanUser.Domain;

public interface IUnbanUserService
{
    Task UnbanUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
