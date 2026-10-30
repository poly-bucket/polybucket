using PolyBucket.Api.Features.SystemSettings.CheckFirstTimeSetup.Repository;
using PolyBucket.Database.Entities.SystemSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.SystemSettings.CheckFirstTimeSetup.Domain
{
    public class CheckFirstTimeSetupService(
        ICheckFirstTimeSetupRepository repository,
        ILogger<CheckFirstTimeSetupService> logger) : ICheckFirstTimeSetupService
    {
        private readonly ICheckFirstTimeSetupRepository _repository = repository;
        private readonly ILogger<CheckFirstTimeSetupService> _logger = logger;

        public async Task<CheckFirstTimeSetupResponse> CheckFirstTimeSetupAsync(CheckFirstTimeSetupQuery request, CancellationToken cancellationToken = default)
        {
            var systemSetup = await _repository.Context.SystemSetups.FirstOrDefaultAsync(cancellationToken);
            
            if (systemSetup == null)
            {
                systemSetup = new SystemSetup
                {
                    Id = Guid.NewGuid(),
                    IsFirstTimeSetup = true,
                    IsAdminConfigured = false,
                    IsSiteConfigured = false,
                    IsStorageConfigured = false,
                    IsEmailConfigured = false,
                    IsModerationConfigured = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _repository.Context.SystemSetups.Add(systemSetup);
                await _repository.Context.SaveChangesAsync(cancellationToken);
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
                currentStep = "storage";
            }

            if (systemSetup.IsStorageConfigured)
            {
                completedSteps++;
                currentStep = "email";
            }

            if (systemSetup.IsEmailConfigured)
            {
                completedSteps++;
                currentStep = "complete";
            }

            return new CheckFirstTimeSetupResponse
            {
                IsFirstTimeSetup = systemSetup.IsFirstTimeSetup,
                IsAdminConfigured = systemSetup.IsAdminConfigured,
                IsSiteConfigured = systemSetup.IsSiteConfigured,
                IsStorageConfigured = systemSetup.IsStorageConfigured,
                IsEmailConfigured = systemSetup.IsEmailConfigured,
                IsModerationConfigured = systemSetup.IsModerationConfigured,
                CurrentStep = currentStep,
                TotalSteps = 5,
                CompletedSteps = completedSteps
            };
        }
    }
}
