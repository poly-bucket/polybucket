using System;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.ACL.Domain;

namespace PolyBucket.Api.Features.Users.CreateUser.Repository;

public interface ICreateUserRepository
{
    Task<Role?> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken = default);
}
