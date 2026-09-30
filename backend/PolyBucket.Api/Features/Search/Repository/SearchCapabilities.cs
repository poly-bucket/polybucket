using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Search.Domain;

namespace PolyBucket.Api.Features.Search.Repository;

public class SearchCapabilities(TimeProvider timeProvider, ILogger<SearchCapabilities> logger) : ISearchCapabilities
{
    public static readonly TimeSpan RecheckInterval = TimeSpan.FromMinutes(5);

    private readonly SemaphoreSlim _lock = new(1, 1);
    private SearchTextMode? _mode;
    private DateTimeOffset _checkedAt;
    private bool _warned;

    public async Task<SearchTextMode> GetModeAsync(PolyBucketDbContext context, CancellationToken cancellationToken = default)
    {
        if (!context.Database.IsNpgsql())
        {
            return SearchTextMode.Basic;
        }

        var now = timeProvider.GetUtcNow();
        if (_mode.HasValue && (_mode == SearchTextMode.Trigram || now - _checkedAt < RecheckInterval))
        {
            return _mode.Value;
        }

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_mode.HasValue && (_mode == SearchTextMode.Trigram || now - _checkedAt < RecheckInterval))
            {
                return _mode.Value;
            }

            _mode = await DetectAsync(context, cancellationToken);
            _checkedAt = now;
            return _mode.Value;
        }
        finally
        {
            _lock.Release();
        }
    }

    public void Invalidate()
    {
        _mode = null;
    }

    private async Task<SearchTextMode> DetectAsync(PolyBucketDbContext context, CancellationToken cancellationToken)
    {
        try
        {
            var installed = await context.Database
                .SqlQueryRaw<bool>("SELECT EXISTS (SELECT 1 FROM pg_extension WHERE extname = 'pg_trgm') AS \"Value\"")
                .SingleAsync(cancellationToken);

            if (installed)
            {
                if (_warned)
                {
                    logger.LogInformation("pg_trgm is now available; search switched to trigram matching");
                }
                _warned = false;
                return SearchTextMode.Trigram;
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogDebug(ex, "Could not query pg_extension for pg_trgm");
        }

        if (!_warned)
        {
            logger.LogWarning("The pg_trgm PostgreSQL extension is not installed. Search falls back to substring matching without typo tolerance. Run CREATE EXTENSION pg_trgm as a database owner to enable fuzzy search.");
            _warned = true;
        }

        return SearchTextMode.Basic;
    }
}
