using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.ACL.Domain;

namespace PolyBucket.Api.Features.Users.CreateUser.Repository;

public class CreateUserRepository(PolyBucketDbContext context) : ICreateUserRepository
{
    public Task<Role?> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);
    }
}
