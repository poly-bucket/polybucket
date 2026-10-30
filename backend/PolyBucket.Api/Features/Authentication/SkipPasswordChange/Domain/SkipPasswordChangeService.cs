using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Authentication.SkipPasswordChange.Repository;
using System.Security.Claims;

namespace PolyBucket.Api.Features.Authentication.SkipPasswordChange.Domain;

public interface ISkipPasswordChangeService
{
    Task<SkipPasswordChangeResponse> SkipPasswordChangeAsync(CancellationToken cancellationToken);
}

public class SkipPasswordChangeResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool RequiresFirstTimeSetup { get; set; }
}

public class SkipPasswordChangeService(
    ISkipPasswordChangeRepository repository,
    IHttpContextAccessor httpContextAccessor,
    ILogger<SkipPasswordChangeService> logger) : ISkipPasswordChangeService
{
    private readonly ISkipPasswordChangeRepository _repository = repository;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly ILogger<SkipPasswordChangeService> _logger = logger;

    public async Task<SkipPasswordChangeResponse> SkipPasswordChangeAsync(CancellationToken cancellationToken)
    {
        try
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return new SkipPasswordChangeResponse
                {
                    Success = false,
                    Message = "User not authenticated",
                    RequiresFirstTimeSetup = false
                };
            }

            var user = await _repository.GetUserForSkipAsync(userId, cancellationToken);
            if (user == null)
            {
                return new SkipPasswordChangeResponse
                {
                    Success = false,
                    Message = "User not found",
                    RequiresFirstTimeSetup = false
                };
            }

            if (user.Role?.Name != "Admin" || user.HasCompletedFirstTimeSetup)
            {
                return new SkipPasswordChangeResponse
                {
                    Success = false,
                    Message = "Password change can only be skipped by admin users during first-time setup",
                    RequiresFirstTimeSetup = false
                };
            }

            await _repository.MarkAdminConfiguredAsync(userId, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Password change skipped for admin user {UserId}", userId);

            return new SkipPasswordChangeResponse
            {
                Success = true,
                Message = "Password change skipped successfully",
                RequiresFirstTimeSetup = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to skip password change for user");
            return new SkipPasswordChangeResponse
            {
                Success = false,
                Message = "Failed to skip password change. Please try again.",
                RequiresFirstTimeSetup = false
            };
        }
    }
}
