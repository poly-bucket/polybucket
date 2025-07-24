using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.SystemSettings.Domain;

namespace PolyBucket.Api.Features.SystemSettings.CheckFirstTimeSetup.Domain
{
    public class CheckFirstTimeSetupQueryHandler(
        PolyBucketDbContext context,
        ILogger<CheckFirstTimeSetupQueryHandler> logger) : IRequestHandler<CheckFirstTimeSetupQuery, CheckFirstTimeSetupResponse>
    {
        private readonly PolyBucketDbContext _context = context;
        private readonly ILogger<CheckFirstTimeSetupQueryHandler> _logger = logger;

        public async Task<CheckFirstTimeSetupResponse> Handle(CheckFirstTimeSetupQuery request, CancellationToken cancellationToken)
        {
            var systemSetup = await _context.SystemSetups.FirstOrDefaultAsync(cancellationToken);
            
            if (systemSetup == null)
            {
                // Create default system setup if none exists
                systemSetup = new SystemSetup
                {
                    Id = Guid.NewGuid(),
                    IsFirstTimeSetup = true,
                    IsAdminConfigured = false,
                    IsSiteConfigured = false,
                    IsEmailConfigured = false,
                    IsModerationConfigured = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _context.SystemSetups.Add(systemSetup);
                await _context.SaveChangesAsync(cancellationToken);
            }

            var completedSteps = 0;
            var currentStep = "admin";

            if (systemSetup.IsAdminConfigured)
            {
                completedSteps++;
                currentStep = "site";
            }

            if (systemSetup.IsSiteConfigured)
            {
                completedSteps++;
                currentStep = "email";
            }

            if (systemSetup.IsEmailConfigured)
            {
                completedSteps++;
                currentStep = "moderation";
            }

            if (systemSetup.IsModerationConfigured)
            {
                completedSteps++;
                currentStep = "complete";
            }

            return new CheckFirstTimeSetupResponse
            {
                IsFirstTimeSetup = systemSetup.IsFirstTimeSetup,
                IsAdminConfigured = systemSetup.IsAdminConfigured,
                IsSiteConfigured = systemSetup.IsSiteConfigured,
                IsEmailConfigured = systemSetup.IsEmailConfigured,
                IsModerationConfigured = systemSetup.IsModerationConfigured,
                CurrentStep = currentStep,
                TotalSteps = 4,
                CompletedSteps = completedSteps
            };
        }
    }
} 