using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using PolyBucket.Api.Common.Storage;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Domain;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Repository;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Services;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.GenerateModelPreview;

public class ModelPreviewProcessorTests
{
    private static readonly Guid ModelId = Guid.NewGuid();

    private readonly Mock<IModelPreviewQueueRepository> _repository = new();
    private readonly Mock<IModelPreviewGenerationService> _generator = new();
    private readonly Mock<IStorageService> _storage = new();
    private readonly ModelPreviewOptions _options = new() { BatchSize = 2, MaxAttempts = 3, LockSeconds = 600 };
    private readonly ModelPreviewProcessor _processor;

    public ModelPreviewProcessorTests()
    {
        _storage
            .Setup(s => s.GetPresignedUrlAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string key, TimeSpan _, CancellationToken _) => $"https://storage.test/{key}");
        _repository
            .Setup(r => r.GetCandidateFilesAsync(ModelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ModelPreviewSourceFile>
            {
                new("readme.pdf", "models/m/readme.pdf"),
                new("part.stl", "models/m/part.stl")
            });
        _processor = new ModelPreviewProcessor(
            _repository.Object,
            _generator.Object,
            _storage.Object,
            Options.Create(_options),
            TimeProvider.System,
            NullLogger<ModelPreviewProcessor>.Instance);
    }

    private ModelPreview GivenClaimed(int attempts, string storageKey = "")
    {
        var preview = new ModelPreview { Id = Guid.NewGuid(), ModelId = ModelId, Size = "thumbnail", Status = PreviewStatus.Generating, Attempts = attempts, StorageKey = storageKey };
        _repository
            .Setup(r => r.ClaimBatchAsync(2, TimeSpan.FromSeconds(600), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ModelPreview> { preview });
        return preview;
    }

    private void GivenGeneratorReturns(PreviewStatus status, string? error = null)
    {
        _generator
            .Setup(g => g.GeneratePreviewAsync(ModelId, It.IsAny<string>(), It.IsAny<string>(), "thumbnail", It.IsAny<PreviewGenerationSettings>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ModelPreview { Status = status, ErrorMessage = error, StorageKey = "previews/new.png", PreviewUrl = "https://storage.test/previews/new.png" });
    }

    [Fact(DisplayName = "When nothing is pending, the processor reports zero and does not render.")]
    public async Task NothingClaimed_ReturnsZero()
    {
        // Arrange
        _repository
            .Setup(r => r.ClaimBatchAsync(It.IsAny<int>(), It.IsAny<TimeSpan>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ModelPreview>());

        // Act
        var processed = await _processor.ProcessPendingAsync();

        // Assert
        processed.ShouldBe(0);
        _generator.VerifyNoOtherCalls();
    }

    [Fact(DisplayName = "When rendering succeeds, the preview is marked completed using the first renderable file.")]
    public async Task Success_MarksCompleted()
    {
        // Arrange
        var preview = GivenClaimed(attempts: 1);
        GivenGeneratorReturns(PreviewStatus.Completed);

        // Act
        var processed = await _processor.ProcessPendingAsync();

        // Assert
        processed.ShouldBe(1);
        _generator.Verify(g => g.GeneratePreviewAsync(ModelId, "https://storage.test/models/m/part.stl", "part.stl", "thumbnail", It.IsAny<PreviewGenerationSettings>(), It.IsAny<CancellationToken>()), Times.Once);
        _repository.Verify(r => r.MarkCompletedAsync(preview.Id, It.Is<ModelPreview>(p => p.StorageKey == "previews/new.png"), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        _repository.Verify(r => r.MarkRetryAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "When a regenerated preview succeeds, the previous image is deleted from storage.")]
    public async Task Success_DeletesReplacedImage()
    {
        // Arrange
        GivenClaimed(attempts: 1, storageKey: "previews/old.png");
        GivenGeneratorReturns(PreviewStatus.Completed);

        // Act
        await _processor.ProcessPendingAsync();

        // Assert
        _storage.Verify(s => s.DeleteAsync("previews/old.png", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "When rendering fails before the attempt limit, the preview is scheduled for a retry.")]
    public async Task Failure_BeforeLimit_SchedulesRetry()
    {
        // Arrange
        var preview = GivenClaimed(attempts: 1);
        GivenGeneratorReturns(PreviewStatus.Failed, "Renderer unavailable");
        var before = DateTime.UtcNow;

        // Act
        await _processor.ProcessPendingAsync();

        // Assert
        _repository.Verify(r => r.MarkRetryAsync(preview.Id, "Renderer unavailable", It.Is<DateTime>(d => d >= before.AddSeconds(59)), It.IsAny<CancellationToken>()), Times.Once);
        _repository.Verify(r => r.MarkFailedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "When the generator throws, the error is recorded and a retry is scheduled.")]
    public async Task GeneratorThrows_SchedulesRetry()
    {
        // Arrange
        var preview = GivenClaimed(attempts: 2);
        _generator
            .Setup(g => g.GeneratePreviewAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PreviewGenerationSettings>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Chromium crashed"));

        // Act
        await _processor.ProcessPendingAsync();

        // Assert
        _repository.Verify(r => r.MarkRetryAsync(preview.Id, "Chromium crashed", It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "When rendering fails on the final attempt, the preview is marked failed.")]
    public async Task Failure_AtLimit_MarksFailed()
    {
        // Arrange
        var preview = GivenClaimed(attempts: 3);
        GivenGeneratorReturns(PreviewStatus.Failed, "Renderer unavailable");

        // Act
        await _processor.ProcessPendingAsync();

        // Assert
        _repository.Verify(r => r.MarkFailedAsync(preview.Id, "Renderer unavailable", It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        _repository.Verify(r => r.MarkRetryAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "When a stale lock is reclaimed past the attempt limit, the preview fails without rendering again.")]
    public async Task StaleLock_PastLimit_FailsWithoutRendering()
    {
        // Arrange
        var preview = GivenClaimed(attempts: 4);

        // Act
        await _processor.ProcessPendingAsync();

        // Assert
        _repository.Verify(r => r.MarkFailedAsync(preview.Id, It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        _generator.VerifyNoOtherCalls();
    }

    [Fact(DisplayName = "When the model has no renderable file, the preview fails permanently without rendering.")]
    public async Task NoRenderableFile_FailsPermanently()
    {
        // Arrange
        var preview = GivenClaimed(attempts: 1);
        _repository
            .Setup(r => r.GetCandidateFilesAsync(ModelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ModelPreviewSourceFile> { new("part.step", "models/m/part.step") });

        // Act
        await _processor.ProcessPendingAsync();

        // Assert
        _repository.Verify(r => r.MarkFailedAsync(preview.Id, It.Is<string>(e => e.Contains("No previewable file")), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
        _generator.VerifyNoOtherCalls();
    }

    [Theory(DisplayName = "When computing retry delays, they double per attempt and cap at one hour.")]
    [InlineData(1, 60)]
    [InlineData(2, 120)]
    [InlineData(3, 240)]
    [InlineData(20, 3600)]
    public void ComputeRetryDelay_DoublesAndCaps(int attempts, int expectedSeconds)
    {
        // Arrange
        var expected = TimeSpan.FromSeconds(expectedSeconds);

        // Act
        var delay = ModelPreviewProcessor.ComputeRetryDelay(attempts);

        // Assert
        delay.ShouldBe(expected);
    }
}
