using System;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Database.Entities.Models;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.InitializeTwoFactorAuth.Repository;

public interface IInitializeTwoFactorAuthUserReadRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
