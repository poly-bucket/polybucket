using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Users.GetBannedUsers.Http;

namespace PolyBucket.Api.Features.Users.GetBannedUsers.Repository;

public class GetBannedUsersRepository(PolyBucketDbContext context) : IGetBannedUsersRepository
{
    public async Task<BannedUsersListResponse> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = context.Users
            .Where(u => u.IsBanned)
            .Include(u => u.BannedByUser)
            .Include(u => u.Role)
            .OrderByDescending(u => u.BannedAt)
            .AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var users = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new BannedUserListItemDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                BannedAt = u.BannedAt,
                BannedByUsername = u.BannedByUser != null ? u.BannedByUser.Username : null,
                BanReason = u.BanReason,
                BanExpiresAt = u.BanExpiresAt,
                RoleName = u.Role != null ? u.Role.Name : "Unknown"
            })
            .ToListAsync(cancellationToken);

        return new BannedUsersListResponse
        {
            Users = users,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages
        };
    }
}
