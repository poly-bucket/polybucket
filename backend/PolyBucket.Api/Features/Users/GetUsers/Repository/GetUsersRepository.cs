using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Users.GetUsers.Domain;

namespace PolyBucket.Api.Features.Users.GetUsers.Repository;

public class GetUsersRepository(PolyBucketDbContext context) : IGetUsersRepository
{
    public async Task<GetUsersResult> GetPagedAsync(GetUsersQuery query, CancellationToken cancellationToken = default)
    {
        var usersQuery = context.Users
            .Include(u => u.Role)
            .Include(u => u.Logins)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.SearchQuery))
        {
            var searchTerm = query.SearchQuery.ToLower();
            usersQuery = usersQuery.Where(u =>
                u.Username.ToLower().Contains(searchTerm) ||
                u.Email.ToLower().Contains(searchTerm) ||
                (u.FirstName != null && u.FirstName.ToLower().Contains(searchTerm)) ||
                (u.LastName != null && u.LastName.ToLower().Contains(searchTerm))
            );
        }

        if (!string.IsNullOrWhiteSpace(query.RoleFilter) && query.RoleFilter != "All Roles")
        {
            usersQuery = usersQuery.Where(u => u.Role != null && u.Role.Name == query.RoleFilter);
        }

        if (!string.IsNullOrWhiteSpace(query.StatusFilter) && query.StatusFilter != "All Status")
        {
            if (query.StatusFilter == "Active")
            {
                usersQuery = usersQuery.Where(u => !u.IsBanned);
            }
            else if (query.StatusFilter == "Banned")
            {
                usersQuery = usersQuery.Where(u => u.IsBanned);
            }
        }

        usersQuery = query.SortBy?.ToLower() switch
        {
            "username" => query.SortDescending ? usersQuery.OrderByDescending(u => u.Username) : usersQuery.OrderBy(u => u.Username),
            "email" => query.SortDescending ? usersQuery.OrderByDescending(u => u.Email) : usersQuery.OrderBy(u => u.Email),
            "role" => query.SortDescending ? usersQuery.OrderByDescending(u => u.Role != null ? u.Role.Name : "") : usersQuery.OrderBy(u => u.Role != null ? u.Role.Name : ""),
            "lastlogin" => query.SortDescending
                ? usersQuery.OrderByDescending(u => u.LastLoginAt)
                : usersQuery.OrderBy(u => u.LastLoginAt),
            "createdat" => query.SortDescending ? usersQuery.OrderByDescending(u => u.CreatedAt) : usersQuery.OrderBy(u => u.CreatedAt),
            _ => query.SortDescending ? usersQuery.OrderByDescending(u => u.CreatedAt) : usersQuery.OrderBy(u => u.CreatedAt)
        };

        var totalCount = await usersQuery.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);

        var users = await usersQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(u => new UserListItemDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Country = u.Country,
                RoleName = u.Role != null ? u.Role.Name : "No Role",
                RoleId = u.RoleId,
                IsBanned = u.IsBanned,
                BannedAt = u.BannedAt,
                BanReason = u.BanReason,
                BanExpiresAt = u.BanExpiresAt,
                HasCompletedFirstTimeSetup = u.HasCompletedFirstTimeSetup,
                RequiresPasswordChange = u.RequiresPasswordChange,
                Avatar = u.Avatar,
                LastLoginAt = null,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt ?? DateTime.UtcNow
            })
            .ToListAsync(cancellationToken);

        return new GetUsersResult
        {
            Users = users,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = totalPages
        };
    }
}
