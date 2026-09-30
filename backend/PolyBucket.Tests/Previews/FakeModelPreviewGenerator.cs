using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Domain;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Services;

namespace PolyBucket.Tests.Previews;

public class FakeModelPreviewGenerator : IModelPreviewGenerationService
{
    private int _failuresRemaining;

    public ConcurrentQueue<(Guid ModelId, string SourceUrl, string FileName, string Size)> Calls { get; } = new();

    public void FailNext(int count)
    {
        Interlocked.Exchange(ref _failuresRemaining, count);
    }

    public void Reset()
    {
        Interlocked.Exchange(ref _failuresRemaining, 0);
        Calls.Clear();
    }

    public Task<ModelPreview> GeneratePreviewAsync(
        Guid modelId,
        string modelFileUrl,
        string fileType,
        string size,
        PreviewGenerationSettings settings,
        CancellationToken cancellationToken = default)
    {
        Calls.Enqueue((modelId, modelFileUrl, fileType, size));

        if (Interlocked.Decrement(ref _failuresRemaining) >= 0)
        {
            return Task.FromResult(new ModelPreview
            {
                ModelId = modelId,
                Size = size,
                Status = PreviewStatus.Failed,
                ErrorMessage = "Renderer unavailable"
            });
        }

        Interlocked.Exchange(ref _failuresRemaining, 0);
        return Task.FromResult(new ModelPreview
        {
            ModelId = modelId,
            Size = size,
            Status = PreviewStatus.Completed,
            PreviewUrl = $"https://storage.test/previews/{modelId}/{size}.png",
            StorageKey = $"previews/{modelId}/{size}.png",
            GeneratedAt = DateTime.UtcNow,
            Width = 800,
            Height = 600,
            FileSizeBytes = 1234
        });
    }

    public Task<bool> IsSupportedFileTypeAsync(string fileType)
    {
        return Task.FromResult(true);
    }
}
