using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PolyBucket.Api.Common.Storage;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Repository;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Services;

namespace PolyBucket.Api.Features.Models.GenerateModelPreview.Domain;

public interface IModelPreviewProcessor
{
    Task<int> ProcessPendingAsync(CancellationToken cancellationToken = default);
}

public class ModelPreviewProcessor(
    IModelPreviewQueueRepository repository,
    IModelPreviewGenerationService generator,
    IStorageService storage,
    IOptions<ModelPreviewOptions> options,
    TimeProvider timeProvider,
    ILogger<ModelPreviewProcessor> logger) : IModelPreviewProcessor
{
    public static readonly string[] PreviewableExtensions = [".stl", ".glb", ".gltf", ".obj"];

    private static readonly TimeSpan SourceUrlLifetime = TimeSpan.FromHours(1);
    private static readonly TimeSpan BaseRetryDelay = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan MaxRetryDelay = TimeSpan.FromHours(1);

    public async Task<int> ProcessPendingAsync(CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        var batch = await repository.ClaimBatchAsync(
            Math.Max(1, settings.BatchSize),
            TimeSpan.FromSeconds(settings.LockSeconds),
            timeProvider.GetUtcNow().UtcDateTime,
            cancellationToken);

        foreach (var preview in batch)
        {
            await ProcessAsync(preview, settings, cancellationToken);
        }

        return batch.Count;
    }

    internal static TimeSpan ComputeRetryDelay(int attempts)
    {
        var exponent = Math.Clamp(attempts - 1, 0, 10);
        var delay = BaseRetryDelay.TotalSeconds * Math.Pow(2, exponent);
        return TimeSpan.FromSeconds(Math.Min(delay, MaxRetryDelay.TotalSeconds));
    }

    private async Task ProcessAsync(ModelPreview preview, ModelPreviewOptions settings, CancellationToken cancellationToken)
    {
        if (preview.Attempts > settings.MaxAttempts)
        {
            await repository.MarkFailedAsync(preview.Id, preview.ErrorMessage ?? "Preview generation did not finish before its lock expired", Now(), CancellationToken.None);
            return;
        }

        var candidates = await repository.GetCandidateFilesAsync(preview.ModelId, cancellationToken);
        var source = candidates.FirstOrDefault(f => PreviewableExtensions.Contains(Path.GetExtension(f.Name).ToLowerInvariant()));
        if (source == null)
        {
            logger.LogInformation("Model {ModelId} has no file that can be rendered as a preview", preview.ModelId);
            await repository.MarkFailedAsync(preview.Id, $"No previewable file ({string.Join(", ", PreviewableExtensions)})", Now(), CancellationToken.None);
            return;
        }

        string error;
        try
        {
            var sourceUrl = await storage.GetPresignedUrlAsync(source.StorageKey, SourceUrlLifetime, cancellationToken);
            var result = await generator.GeneratePreviewAsync(
                preview.ModelId,
                sourceUrl,
                source.Name,
                preview.Size,
                new PreviewGenerationSettings(),
                cancellationToken);

            if (result.Status == PreviewStatus.Completed)
            {
                await repository.MarkCompletedAsync(preview.Id, result, Now(), CancellationToken.None);
                await DeleteReplacedImageAsync(preview.StorageKey, result.StorageKey);
                logger.LogInformation("Generated {Size} preview for model {ModelId} on attempt {Attempt}", preview.Size, preview.ModelId, preview.Attempts);
                return;
            }

            error = result.ErrorMessage ?? "Preview generation failed";
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            error = ex.Message;
        }

        if (preview.Attempts >= settings.MaxAttempts)
        {
            logger.LogWarning("Preview for model {ModelId} failed after {Attempts} attempt(s): {Error}", preview.ModelId, preview.Attempts, error);
            await repository.MarkFailedAsync(preview.Id, error, Now(), CancellationToken.None);
            return;
        }

        var nextAttempt = Now().Add(ComputeRetryDelay(preview.Attempts));
        logger.LogWarning("Preview for model {ModelId} failed on attempt {Attempts}; retrying at {NextAttempt}: {Error}", preview.ModelId, preview.Attempts, nextAttempt, error);
        await repository.MarkRetryAsync(preview.Id, error, nextAttempt, CancellationToken.None);
    }

    private async Task DeleteReplacedImageAsync(string previousKey, string newKey)
    {
        if (string.IsNullOrEmpty(previousKey) || previousKey == newKey)
        {
            return;
        }

        try
        {
            await storage.DeleteAsync(previousKey, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not delete replaced preview image {StorageKey}", previousKey);
        }
    }

    private DateTime Now() => timeProvider.GetUtcNow().UtcDateTime;
}
