using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.SystemSettings.Domain;
using System.Security.Claims;

namespace PolyBucket.Api.Features.SystemSettings.CompleteFirstTimeSetup.Domain
{
    public class CompleteFirstTimeSetupCommandHandler(
        PolyBucketDbContext context,
        ILogger<CompleteFirstTimeSetupCommandHandler> logger,
        IHttpContextAccessor httpContextAccessor) : IRequestHandler<CompleteFirstTimeSetupCommand, CompleteFirstTimeSetupResponse>
    {
        private readonly PolyBucketDbContext _context = context;
        private readonly ILogger<CompleteFirstTimeSetupCommandHandler> _logger = logger;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public async Task<CompleteFirstTimeSetupResponse> Handle(CompleteFirstTimeSetupCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var systemSetup = await _context.SystemSetups.FirstOrDefaultAsync(cancellationToken);
                
                if (systemSetup == null)
                {
                    return new CompleteFirstTimeSetupResponse
                    {
                        Success = false,
                        Message = "System setup not found",
                        CompletedAt = DateTime.UtcNow
                    };
                }

                // Verify required steps are completed (admin and site configuration are required)
                if (!systemSetup.IsAdminConfigured || !systemSetup.IsSiteConfigured)
                {
                    return new CompleteFirstTimeSetupResponse
                    {
                        Success = false,
                        Message = "Admin password change and site settings must be completed before finishing",
                        CompletedAt = DateTime.UtcNow
                    };
                }

                // Mark setup as complete
                systemSetup.IsFirstTimeSetup = false;
                systemSetup.SetupCompletedAt = DateTime.UtcNow;
                systemSetup.UpdatedAt = DateTime.UtcNow;
                
                // Get current user ID if available
                var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    systemSetup.SetupCompletedByUserId = userId;
                    systemSetup.UpdatedById = userId;
                }

                await _context.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("First-time setup completed successfully by user {UserId}", 
                    userIdClaim?.Value ?? "unknown");

                return new CompleteFirstTimeSetupResponse
                {
                    Success = true,
                    Message = "First-time setup completed successfully",
                    CompletedAt = systemSetup.SetupCompletedAt.Value
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to complete first-time setup");
                return new CompleteFirstTimeSetupResponse
                {
                    Success = false,
                    Message = "Failed to complete first-time setup. Please try again.",
                    CompletedAt = DateTime.UtcNow
                };
            }
        }
    }
} 