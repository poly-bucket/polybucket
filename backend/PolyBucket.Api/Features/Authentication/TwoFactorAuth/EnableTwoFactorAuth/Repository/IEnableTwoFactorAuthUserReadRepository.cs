using System;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Database.Entities.Models;

namespace PolyBucket.Api.Features.Authentication.TwoFactorAuth.EnableTwoFactorAuth.Repository;

public interface IEnableTwoFactorAuthUserReadRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
