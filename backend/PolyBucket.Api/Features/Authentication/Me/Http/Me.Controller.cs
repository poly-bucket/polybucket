using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Common;
using PolyBucket.Api.Common.Email;
using PolyBucket.Api.Common.Models;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.Me.Http
{
    [ApiController]
    [Route("api/auth")]
    [Authorize]
    public class MeController(PolyBucketDbContext context, IEmailSettingsResolver emailSettingsResolver) : ControllerBase
    {
        private readonly PolyBucketDbContext _context = context;
        private readonly IEmailSettingsResolver _emailSettingsResolver = emailSettingsResolver;

        /// <summary>
        /// Returns the profile of the signed-in user, including email verification state.
        /// </summary>
        /// <response code="200">The current user.</response>
        /// <response code="401">The access token is missing or invalid.</response>
        /// <response code="404">The user no longer exists.</response>
        [HttpGet("me")]
        [ProducesResponseType(200, Type = typeof(MeResponse))]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<MeResponse>> GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindUserIdClaim();
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized("Invalid authentication token");
                }

                var user = await _context.Users
                    .Include(u => u.Role)
                    .Include(u => u.Settings)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return NotFound("User not found");
                }

                var emailSettings = await _emailSettingsResolver.GetEffectiveSettingsAsync(HttpContext.RequestAborted);

                var response = new MeResponse
                {
                    EmailDeliveryAvailable = emailSettings.CanDeliver,
                    EmailVerificationRequired = emailSettings.CanDeliver && emailSettings.RequireEmailVerification,
                    Id = user.Id.ToString(),
                    Username = user.Username ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = user.Role?.Name ?? "User",
                    IsEmailVerified = user.EmailVerifiedAt.HasValue,
                    EmailVerifiedAt = user.EmailVerifiedAt,
                    PendingEmail = user.PendingEmail,
                    CreatedAt = user.CreatedAt,
                    RequiresPasswordChange = user.RequiresPasswordChange,
                    HasCompletedFirstTimeSetup = user.HasCompletedFirstTimeSetup,
                    Avatar = user.Avatar,
                    ProfilePictureUrl = user.ProfilePictureUrl,
                    Settings = user.Settings != null ? new UserSettingsResponse
                    {
                        Language = user.Settings.Language,
                        Theme = user.Settings.Theme,
                        EmailNotifications = user.Settings.EmailNotifications,
                        MeasurementSystem = user.Settings.MeasurementSystem,
                        TimeZone = user.Settings.TimeZone
                    } : null
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving user information" });
            }
        }
    }

    public class MeResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool IsEmailVerified { get; set; }
        public DateTime? EmailVerifiedAt { get; set; }
        public string? PendingEmail { get; set; }
        public bool EmailVerificationRequired { get; set; }
        public bool EmailDeliveryAvailable { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool RequiresPasswordChange { get; set; }
        public bool HasCompletedFirstTimeSetup { get; set; }
        public string? Avatar { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public UserSettingsResponse? Settings { get; set; }
    }

    public class UserSettingsResponse
    {
        public string Language { get; set; } = string.Empty;
        public string Theme { get; set; } = string.Empty;
        public bool EmailNotifications { get; set; }
        public string MeasurementSystem { get; set; } = string.Empty;
        public string TimeZone { get; set; } = string.Empty;
    }
}
