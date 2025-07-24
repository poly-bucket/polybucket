using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Users.Commands
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize]
    [RequirePermission(PermissionConstants.ADMIN_BAN_USERS)]
    public class BanUserController(PolyBucketDbContext context) : ControllerBase
    {
        private readonly PolyBucketDbContext _context = context;

        /// <summary>
        /// Ban a user
        /// </summary>
        /// <param name="userId">User ID to ban</param>
        /// <param name="request">Ban details</param>
        /// <returns>Result of the ban operation</returns>
        [HttpPost("{userId}/ban")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> BanUser(Guid userId, [FromBody] BanUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(currentUserIdStr, out var currentUserId))
            {
                return Unauthorized("Invalid user token");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            if (user.IsBanned)
            {
                return BadRequest("User is already banned");
            }

            // Cannot ban self
            if (user.Id == currentUserId)
            {
                return BadRequest("Cannot ban yourself");
            }

            user.IsBanned = true;
            user.BannedAt = DateTime.UtcNow;
            user.BannedById = currentUserId;
            user.BanReason = request.Reason;
            user.BanExpiresAt = request.ExpiresAt;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "User banned successfully" });
        }

        /// <summary>
        /// Unban a user
        /// </summary>
        /// <param name="userId">User ID to unban</param>
        /// <returns>Result of the unban operation</returns>
        [HttpPost("{userId}/unban")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UnbanUser(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            if (!user.IsBanned)
            {
                return BadRequest("User is not banned");
            }

            user.IsBanned = false;
            user.BannedAt = null;
            user.BannedById = null;
            user.BanReason = null;
            user.BanExpiresAt = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "User unbanned successfully" });
        }

        /// <summary>
        /// Get all banned users
        /// </summary>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Items per page</param>
        /// <returns>List of banned users</returns>
        [HttpGet("banned")]
        [ProducesResponseType(typeof(BannedUsersResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetBannedUsers(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 20)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
            {
                return BadRequest("Invalid pagination parameters");
            }

            var query = _context.Users
                .Where(u => u.IsBanned)
                .Include(u => u.BannedByUser)
                .Include(u => u.Role)
                .OrderByDescending(u => u.BannedAt);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new BannedUserDto
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
                .ToListAsync();

            var response = new BannedUsersResponse
            {
                Users = users,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return Ok(response);
        }
    }

    public class BanUserRequest
    {
        [Required]
        [StringLength(500, MinimumLength = 3)]
        public string Reason { get; set; } = string.Empty;

        public DateTime? ExpiresAt { get; set; }
    }

    public class BannedUserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime? BannedAt { get; set; }
        public string? BannedByUsername { get; set; }
        public string? BanReason { get; set; }
        public DateTime? BanExpiresAt { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }

    public class BannedUsersResponse
    {
        public IEnumerable<BannedUserDto> Users { get; set; } = new List<BannedUserDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
} 