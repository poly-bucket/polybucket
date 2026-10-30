using PolyBucket.Api.Features.SystemSettings.CompleteFirstTimeSetup.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PolyBucket.Database.Persistence;
using PolyBucket.Api.Features.SystemSettings.Domain;
using System.Security.Claims;

namespace PolyBucket.Api.Features.SystemSettings.CompleteFirstTimeSetup.Domain
{
    public class CompleteFirstTimeSetupService(
        ICompleteFirstTimeSetupRepository repository,
        ILogger<CompleteFirstTimeSetupService> logger,
        IHttpContextAccessor httpContextAccessor) : ICompleteFirstTimeSetupService
    {
        private readonly ICompleteFirstTimeSetupRepository _repository = repository;
        private readonly ILogger<CompleteFirstTimeSetupService> _logger = logger;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public async Task<CompleteFirstTimeSetupResponse> CompleteFirstTimeSetupAsync(CompleteFirstTimeSetupCommand request, CancellationToken cancellationToken = default)
        {
            try
            {
                var systemSetup = await _repository.Context.SystemSetups.FirstOrDefaultAsync(cancellationToken);
                
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
                if (!systemSetup.IsAdminConfigured || !systemSetup.IsSiteConfigured || !systemSetup.IsStorageConfigured)
                {
                    return new CompleteFirstTimeSetupResponse
                    {
                        Success = false,
                        Message = "Admin account, site settings, and object storage must be completed before finishing setup",
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

                    var completingUser = await _repository.Context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
                    if (completingUser != null)
                    {
                        completingUser.HasCompletedFirstTimeSetup = true;
                    }
                }

                await _repository.Context.SaveChangesAsync(cancellationToken);
                
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