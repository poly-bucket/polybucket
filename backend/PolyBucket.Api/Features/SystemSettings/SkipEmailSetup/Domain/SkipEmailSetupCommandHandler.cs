using MediatR;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Persistence;
using System.Security.Claims;

namespace PolyBucket.Api.Features.SystemSettings.SkipEmailSetup.Domain;

public class SkipEmailSetupCommandHandler(
    PolyBucketDbContext context,
    IHttpContextAccessor httpContextAccessor,
    ILogger<SkipEmailSetupCommandHandler> logger) : IRequestHandler<SkipEmailSetupCommand, SkipEmailSetupResponse>
{
    public async Task<SkipEmailSetupResponse> Handle(SkipEmailSetupCommand request, CancellationToken cancellationToken)
    {
        var systemSetup = await context.SystemSetups.FirstOrDefaultAsync(cancellationToken);
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

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Email setup step skipped during first-time setup");

        return new SkipEmailSetupResponse
        {
            Success = true,
            Message = "Email setup skipped. You can configure email later in system settings."
        };
    }
}
