using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Data;

namespace PolyBucket.Api.Features.Search.Repository;

public class SearchCapabilityStartupCheck(
    IServiceScopeFactory scopeFactory,
    ISearchCapabilities capabilities,
    ILogger<SearchCapabilityStartupCheck> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PolyBucketDbContext>();
            var mode = await capabilities.GetModeAsync(context, stoppingToken);
            logger.LogInformation("Search text matching mode: {Mode}", mode);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogDebug(ex, "Search capability check skipped at startup");
        }
    }
}
