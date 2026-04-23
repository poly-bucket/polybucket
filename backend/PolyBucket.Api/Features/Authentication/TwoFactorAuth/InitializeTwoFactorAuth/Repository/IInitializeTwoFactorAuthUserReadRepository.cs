using System;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Common.Models;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.InitializeTwoFactorAuth.Repository;

public interface IInitializeTwoFactorAuthUserReadRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
