using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Domain;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Repository;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.GenerateModelPreview;

public class ModelPreviewQueueTests
{
    private static readonly Guid ModelId = Guid.NewGuid();

    private readonly Mock<IModelPreviewQueueRepository> _repository = new();
    private readonly Mock<IModelPreviewSignal> _signal = new();
    private readonly ModelPreviewQueue _queue;

    public ModelPreviewQueueTests()
    {
        _repository.Setup(r => r.GetModelAuthorIdAsync(ModelId, It.IsAny<CancellationToken>())).ReturnsAsync(Guid.NewGuid());
        _repository.Setup(r => r.TryAddAsync(It.IsAny<ModelPreview>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _repository.Setup(r => r.IsAutoGenerateEnabledAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _queue = new ModelPreviewQueue(
            _repository.Object,
            _signal.Object,
            Options.Create(new ModelPreviewOptions()),
            TimeProvider.System,
            NullLogger<ModelPreviewQueue>.Instance);
    }

    private ModelPreview GivenExisting(PreviewStatus status)
    {
        var existing = new ModelPreview { Id = Guid.NewGuid(), ModelId = ModelId, Size = "thumbnail", Status = status, Attempts = 3, ErrorMessage = "old" };
        _repository.Setup(r => r.GetAsync(ModelId, "thumbnail", It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        return existing;
    }

    [Fact(DisplayName = "When no preview exists, a pending row is added and the worker is signalled.")]
    public async Task NoExisting_AddsPendingAndSignals()
    {
        // Arrange
        _repository.Setup(r => r.GetAsync(ModelId, "thumbnail", It.IsAny<CancellationToken>())).ReturnsAsync((ModelPreview?)null);

        // Act
        var preview = await _queue.EnqueueAsync(ModelId, "thumbnail", forceRegenerate: false, CancellationToken.None);

        // Assert
        preview.ShouldNotBeNull();
        preview.Status.ShouldBe(PreviewStatus.Pending);
        _repository.Verify(r => r.TryAddAsync(It.Is<ModelPreview>(p => p.ModelId == ModelId && p.Size == "thumbnail"), It.IsAny<CancellationToken>()), Times.Once);
        _signal.Verify(s => s.Notify(), Times.Once);
    }

    [Fact(DisplayName = "When the model does not exist, nothing is queued.")]
    public async Task MissingModel_ReturnsNull()
    {
        // Arrange
        _repository.Setup(r => r.GetModelAuthorIdAsync(ModelId, It.IsAny<CancellationToken>())).ReturnsAsync((Guid?)null);

        // Act
        var preview = await _queue.EnqueueAsync(ModelId, "thumbnail", forceRegenerate: true, CancellationToken.None);

        // Assert
        preview.ShouldBeNull();
        _repository.Verify(r => r.TryAddAsync(It.IsAny<ModelPreview>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "When a failed preview is requested again, it is reset to pending with a fresh attempt budget.")]
    public async Task Failed_IsRequeued()
    {
        // Arrange
        var existing = GivenExisting(PreviewStatus.Failed);

        // Act
        await _queue.EnqueueAsync(ModelId, "thumbnail", forceRegenerate: false, CancellationToken.None);

        // Assert
        existing.Status.ShouldBe(PreviewStatus.Pending);
        existing.Attempts.ShouldBe(0);
        existing.ErrorMessage.ShouldBeNull();
        _repository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _signal.Verify(s => s.Notify(), Times.Once);
    }

    [Fact(DisplayName = "When a completed preview is requested without forcing, it is left as is.")]
    public async Task Completed_WithoutForce_Unchanged()
    {
        // Arrange
        var existing = GivenExisting(PreviewStatus.Completed);

        // Act
        await _queue.EnqueueAsync(ModelId, "thumbnail", forceRegenerate: false, CancellationToken.None);

        // Assert
        existing.Status.ShouldBe(PreviewStatus.Completed);
        _repository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _signal.Verify(s => s.Notify(), Times.Never);
    }

    [Fact(DisplayName = "When a completed preview is force-regenerated, it is reset to pending.")]
    public async Task Completed_WithForce_Requeued()
    {
        // Arrange
        var existing = GivenExisting(PreviewStatus.Completed);

        // Act
        await _queue.EnqueueAsync(ModelId, "thumbnail", forceRegenerate: true, CancellationToken.None);

        // Assert
        existing.Status.ShouldBe(PreviewStatus.Pending);
        _signal.Verify(s => s.Notify(), Times.Once);
    }

    [Fact(DisplayName = "When a preview is already generating, requesting it again does not interrupt the worker.")]
    public async Task Generating_WithForce_Unchanged()
    {
        // Arrange
        var existing = GivenExisting(PreviewStatus.Generating);

        // Act
        await _queue.EnqueueAsync(ModelId, "thumbnail", forceRegenerate: true, CancellationToken.None);

        // Assert
        existing.Status.ShouldBe(PreviewStatus.Generating);
        _repository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "When two requests race to insert the same preview, the loser reuses the winner's row.")]
    public async Task InsertRace_ReusesExistingRow()
    {
        // Arrange
        var winner = new ModelPreview { Id = Guid.NewGuid(), ModelId = ModelId, Size = "thumbnail", Status = PreviewStatus.Pending };
        _repository.SetupSequence(r => r.GetAsync(ModelId, "thumbnail", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ModelPreview?)null)
            .ReturnsAsync(winner);
        _repository.Setup(r => r.TryAddAsync(It.IsAny<ModelPreview>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var preview = await _queue.EnqueueAsync(ModelId, "thumbnail", forceRegenerate: false, CancellationToken.None);

        // Assert
        preview.ShouldBe(winner);
    }

    [Fact(DisplayName = "When automatic previews are turned off in model settings, new content is not queued.")]
    public async Task AutoDisabled_DoesNotQueue()
    {
        // Arrange
        _repository.Setup(r => r.IsAutoGenerateEnabledAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        await _queue.EnqueueForNewContentAsync(ModelId, CancellationToken.None);

        // Assert
        _repository.Verify(r => r.TryAddAsync(It.IsAny<ModelPreview>(), It.IsAny<CancellationToken>()), Times.Never);
        _repository.Verify(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "When queueing for new content fails, the error is swallowed so the upload still succeeds.")]
    public async Task NewContent_RepositoryThrows_DoesNotThrow()
    {
        // Arrange
        _repository.Setup(r => r.IsAutoGenerateEnabledAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("db down"));

        // Act
        var act = () => _queue.EnqueueForNewContentAsync(ModelId, CancellationToken.None);

        // Assert
        await Should.NotThrowAsync(act);
    }
}
