using MediatR;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.SystemSettings.ApplyStorageSetup.Domain;

public class ApplyStorageSetupCommandHandler(
    PolyBucketDbContext context,
    IHostApplicationLifetime lifetime,
    ILogger<ApplyStorageSetupCommandHandler> logger) : IRequestHandler<ApplyStorageSetupCommand, ApplyStorageSetupResponse>
{
    public async Task<ApplyStorageSetupResponse> Handle(ApplyStorageSetupCommand request, CancellationToken cancellationToken)
    {
        var systemSetup = await context.SystemSetups.FirstOrDefaultAsync(cancellationToken);
        if (systemSetup == null || !systemSetup.IsStorageConfigured)
        {
            return new ApplyStorageSetupResponse
            {
                Success = false,
                Message = "Save and test object storage settings before applying.",
                RestartScheduled = false
            };
        }

        logger.LogInformation("Scheduling API restart to apply object storage configuration");

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(1), lifetime.ApplicationStopping).ConfigureAwait(false);
                lifetime.StopApplication();
            }
            catch (OperationCanceledException)
            {
                // shutdown already in progress
            }
        }, CancellationToken.None);

        return new ApplyStorageSetupResponse
        {
            Success = true,
            Message = "Object storage settings saved. The API is restarting to connect to object storage.",
            RestartScheduled = true
        };
    }
}
