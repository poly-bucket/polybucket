using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PolyBucket.Api.Features.Models.GenerateModelPreview.Domain;

public class ModelPreviewWorker(
    IServiceScopeFactory scopeFactory,
    IModelPreviewSignal signal,
    IOptions<ModelPreviewOptions> options,
    ILogger<ModelPreviewWorker> logger) : BackgroundService
{
    private static readonly TimeSpan ErrorBackoff = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.WorkerEnabled)
        {
            logger.LogInformation("Model preview worker is disabled by configuration");
            return;
        }

        var pollInterval = TimeSpan.FromSeconds(Math.Max(1, options.Value.PollSeconds));

        while (!stoppingToken.IsCancellationRequested)
        {
            var processed = 0;
            try
            {
                using var scope = scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<IModelPreviewProcessor>();
                processed = await processor.ProcessPendingAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Model preview worker iteration failed");
                await Task.Delay(ErrorBackoff, stoppingToken).ContinueWith(_ => { }, TaskScheduler.Default);
                continue;
            }

            if (processed == 0)
            {
                await signal.WaitAsync(pollInterval, stoppingToken).ContinueWith(_ => { }, TaskScheduler.Default);
            }
        }
    }
}
