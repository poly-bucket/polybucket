using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Models.RecordModelDownload.Domain;
using PolyBucket.Api.Features.Models.RecordModelDownload.Repository;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.RecordModelDownload;

public class ModelDownloadCounterTests
{
    private readonly Mock<IModelDownloadCounterRepository> _repository = new();
    private readonly ModelDownloadCounter _counter;

    public ModelDownloadCounterTests()
    {
        _counter = new ModelDownloadCounter(_repository.Object, TimeProvider.System);
    }

    [Fact(DisplayName = "When the downloader is the model author, downloads are not counted.")]
    public async Task TryRecordDownload_Owner_ReturnsNotCounted()
    {
        var modelId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        _repository.Setup(r => r.GetModelAsync(modelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Model { Id = modelId, AuthorId = authorId, Downloads = 4, Name = "x" });

        var (downloads, counted) = await _counter.TryRecordDownloadAsync(
            modelId,
            $"u:{authorId}",
            CancellationToken.None);

        downloads.ShouldBe(4);
        counted.ShouldBeFalse();
        _repository.Verify(
            r => r.TryRecordDownloadAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "When the viewer is not the author, the repository records the download.")]
    public async Task TryRecordDownload_NonOwner_RecordsDownload()
    {
        var modelId = Guid.NewGuid();
        var viewerId = Guid.NewGuid();
        _repository.Setup(r => r.GetModelAsync(modelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Model { Id = modelId, AuthorId = Guid.NewGuid(), Downloads = 1, Name = "x" });
        _repository.Setup(r => r.TryRecordDownloadAsync(
                modelId,
                $"u:{viewerId}",
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((2, true));

        var (downloads, counted) = await _counter.TryRecordDownloadAsync(
            modelId,
            $"u:{viewerId}",
            CancellationToken.None);

        downloads.ShouldBe(2);
        counted.ShouldBeTrue();
    }
}
