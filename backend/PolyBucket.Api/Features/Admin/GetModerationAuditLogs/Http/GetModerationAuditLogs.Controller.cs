using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.ModelModeration.Domain;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Admin.GetModerationAuditLogs.Http
{
    [Authorize]
    [ApiController]
    [Route("api/admin/moderation")]
    public class GetModerationAuditLogsController : ControllerBase
    {
        private readonly PolyBucketDbContext _context;

        public GetModerationAuditLogsController(PolyBucketDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get moderation audit logs for admin review
        /// </summary>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Items per page</param>
        /// <param name="action">Filter by action type</param>
        /// <param name="userId">Filter by user ID</param>
        /// <param name="modelId">Filter by model ID</param>
        /// <returns>Paginated audit logs</returns>
        [HttpGet("audit-logs")]
        [ProducesResponseType(typeof(ModerationAuditResponse), 200)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<ModerationAuditResponse>> GetAuditLogs(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? action = null,
            [FromQuery] string? userId = null,
            [FromQuery] string? modelId = null)
        {
            // Get current user ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var currentUserId))
            {
                return Unauthorized("Invalid user token");
            }

            // Check if user has admin privileges
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == currentUserId);
            if (user == null || user.Role == null || user.Role.Name != "Admin")
            {
                return Forbid("Admin privileges required to view audit logs");
            }

            // Build query
            var query = _context.ModerationAuditLogs
                .Include(log => log.PerformedByUser)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(action))
            {
                if (Enum.TryParse<ModerationAction>(action, true, out var actionEnum))
                {
                    query = query.Where(log => log.Action == actionEnum);
                }
            }

            if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var userGuid))
            {
                query = query.Where(log => log.PerformedByUserId == userGuid);
            }

            if (!string.IsNullOrEmpty(modelId) && Guid.TryParse(modelId, out var modelGuid))
            {
                query = query.Where(log => log.ModelId == modelGuid);
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            // Apply pagination and ordering
            var logs = await query
                .OrderByDescending(log => log.PerformedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(log => new ModerationAuditDto
                {
                    Id = log.Id.ToString(),
                    ModelId = log.ModelId.ToString(),
                    PerformedByUserId = log.PerformedByUserId.ToString(),
                    PerformedByUser = new UserInfoDto
                    {
                        Username = log.PerformedByUser.Username,
                        Email = log.PerformedByUser.Email
                    },
                    Action = log.Action.ToString(),
                    PreviousValues = log.PreviousValues,
                    NewValues = log.NewValues,
                    ModerationNotes = log.ModerationNotes,
                    PerformedAt = log.PerformedAt,
                    IPAddress = log.IPAddress,
                    UserAgent = log.UserAgent
                })
                .ToListAsync();

            var response = new ModerationAuditResponse
            {
                Logs = logs,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return Ok(response);
        }
    }

    public class ModerationAuditResponse
    {
        public List<ModerationAuditDto> Logs { get; set; } = new List<ModerationAuditDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class ModerationAuditDto
    {
        public string Id { get; set; } = string.Empty;
        public string ModelId { get; set; } = string.Empty;
        public string PerformedByUserId { get; set; } = string.Empty;
        public UserInfoDto PerformedByUser { get; set; } = null!;
        public string Action { get; set; } = string.Empty;
        public string? PreviousValues { get; set; }
        public string? NewValues { get; set; }
        public string? ModerationNotes { get; set; }
        public DateTime PerformedAt { get; set; }
        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }
    }

    public class UserInfoDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
} 