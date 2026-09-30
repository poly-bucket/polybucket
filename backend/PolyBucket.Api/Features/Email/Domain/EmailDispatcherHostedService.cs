using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PolyBucket.Api.Common.Email;

namespace PolyBucket.Api.Features.Email.Domain;

public class EmailDispatcherHostedService(
    IServiceScopeFactory scopeFactory,
    IEmailDispatchSignal signal,
    IOptions<EmailOptions> options,
    ILogger<EmailDispatcherHostedService> logger) : BackgroundService
{
    private static readonly TimeSpan PurgeInterval = TimeSpan.FromHours(1);
    private static readonly TimeSpan ErrorBackoff = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Dispatcher.Enabled)
        {
            logger.LogInformation("Email dispatcher is disabled by configuration");
            return;
        }

        var pollInterval = TimeSpan.FromSeconds(options.Value.Dispatcher.PollSeconds);
        var lastPurge = DateTime.MinValue;

        while (!stoppingToken.IsCancellationRequested)
        {
            var processed = 0;
            try
            {
                using var scope = scopeFactory.CreateScope();
                var dispatcher = scope.ServiceProvider.GetRequiredService<IEmailDispatcher>();
                processed = await dispatcher.DispatchPendingAsync(stoppingToken);

                if (DateTime.UtcNow - lastPurge > PurgeInterval)
                {
                    var purged = await dispatcher.PurgeSentAsync(stoppingToken);
                    if (purged > 0)
                    {
                        logger.LogInformation("Purged {Count} sent email(s) past the retention window", purged);
                    }

                    lastPurge = DateTime.UtcNow;
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Email dispatcher iteration failed");
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
