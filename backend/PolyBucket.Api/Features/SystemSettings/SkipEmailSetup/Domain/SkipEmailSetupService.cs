using PolyBucket.Api.Features.SystemSettings.SkipEmailSetup.Repository;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Persistence;
using System.Security.Claims;

namespace PolyBucket.Api.Features.SystemSettings.SkipEmailSetup.Domain;

public class SkipEmailSetupService(
    ISkipEmailSetupRepository repository,
    IHttpContextAccessor httpContextAccessor,
    ILogger<SkipEmailSetupService> logger) : ISkipEmailSetupService
{
    public async Task<SkipEmailSetupResponse> SkipEmailSetupAsync(SkipEmailSetupCommand request, CancellationToken cancellationToken = default)
    {
        var systemSetup = await repository.Context.SystemSetups.FirstOrDefaultAsync(cancellationToken);
        if (systemSetup == null || !systemSetup.IsFirstTimeSetup)
        {
            return new SkipEmailSetupResponse
            {
                Success = false,
                Message = "First-time setup is not active."
            };
        }

        systemSetup.IsEmailConfigured = true;
        systemSetup.UpdatedAt = DateTime.UtcNow;

        var userIdClaim = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            systemSetup.UpdatedById = userId;
        }

        await repository.Context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Email setup step skipped during first-time setup");

        return new SkipEmailSetupResponse
        {
            Success = true,
            Message = "Email setup skipped. You can configure email later in system settings."
        };
    }
}
