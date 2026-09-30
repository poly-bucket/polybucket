using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Common.Models.Enums;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.Models.RecordModelView.Domain;
using PolyBucket.Api.Features.Models.RecordModelView.Repository;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.RecordModelView;

public class RecordModelViewServiceTests
{
    private readonly Mock<IRecordModelViewRepository> _repository = new();
    private readonly Mock<IPermissionService> _permissionService = new();
    private readonly RecordModelViewService _service;
    private static readonly string ViewerKey = "u:00000000-0000-0000-0000-000000000001";

    public RecordModelViewServiceTests()
    {
        _service = new RecordModelViewService(
            _repository.Object,
            _permissionService.Object,
            TimeProvider.System);
    }

    [Fact(DisplayName = "When the model does not exist, RecordViewAsync returns NotFound.")]
    public async Task RecordView_MissingModel_ReturnsNotFound()
    {
        var modelId = Guid.NewGuid();
        _repository.Setup(r => r.GetModelForViewAsync(modelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Model?)null);

        var outcome = await _service.RecordViewAsync(
            modelId,
            new ClaimsPrincipal(),
            ViewerKey,
            CancellationToken.None);

        outcome.Kind.ShouldBe(RecordModelViewOutcomeKind.NotFound);
    }

    [Fact(DisplayName = "When the viewer is the model author, the view is not counted.")]
    public async Task RecordView_Owner_ReturnsOkWithoutCounting()
    {
        var modelId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var model = CreatePublicModel(modelId, authorId, views: 5);
        var user = CreateUser(authorId);

        _repository.Setup(r => r.GetModelForViewAsync(modelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(model);

        var outcome = await _service.RecordViewAsync(modelId, user, $"u:{authorId}", CancellationToken.None);

        outcome.Kind.ShouldBe(RecordModelViewOutcomeKind.Ok);
        outcome.Views.ShouldBe(5);
        outcome.Counted.ShouldBeFalse();
        _repository.Verify(
            r => r.TryRecordViewAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "When an anonymous user views a private model, RecordViewAsync returns Forbid.")]
    public async Task RecordView_PrivateModelAnonymous_ReturnsForbid()
    {
        var modelId = Guid.NewGuid();
        var model = CreatePublicModel(modelId, Guid.NewGuid(), views: 0);
        model.Privacy = PrivacySettings.Private;
        model.IsPublic = false;

        _repository.Setup(r => r.GetModelForViewAsync(modelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(model);

        var outcome = await _service.RecordViewAsync(
            modelId,
            new ClaimsPrincipal(),
            "a:abc",
            CancellationToken.None);

        outcome.Kind.ShouldBe(RecordModelViewOutcomeKind.Forbid);
    }

    [Fact(DisplayName = "When a viewer is allowed and dedup passes, the repository records the view.")]
    public async Task RecordView_AllowedViewer_RecordsView()
    {
        var modelId = Guid.NewGuid();
        var viewerId = Guid.NewGuid();
        var model = CreatePublicModel(modelId, Guid.NewGuid(), views: 2);
        var user = CreateUser(viewerId);

        _repository.Setup(r => r.GetModelForViewAsync(modelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(model);
        _repository.Setup(r => r.TryRecordViewAsync(
                modelId,
                $"u:{viewerId}",
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((3, true));

        var outcome = await _service.RecordViewAsync(modelId, user, $"u:{viewerId}", CancellationToken.None);

        outcome.Kind.ShouldBe(RecordModelViewOutcomeKind.Ok);
        outcome.Views.ShouldBe(3);
        outcome.Counted.ShouldBeTrue();
    }

    [Fact(DisplayName = "When dedup suppresses the view, RecordViewAsync returns counted false.")]
    public async Task RecordView_DedupSuppressed_ReturnsNotCounted()
    {
        var modelId = Guid.NewGuid();
        var viewerId = Guid.NewGuid();
        var model = CreatePublicModel(modelId, Guid.NewGuid(), views: 10);
        var user = CreateUser(viewerId);

        _repository.Setup(r => r.GetModelForViewAsync(modelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(model);
        _repository.Setup(r => r.TryRecordViewAsync(
                modelId,
                $"u:{viewerId}",
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((10, false));

        var outcome = await _service.RecordViewAsync(modelId, user, $"u:{viewerId}", CancellationToken.None);

        outcome.Counted.ShouldBeFalse();
        outcome.Views.ShouldBe(10);
    }

    private static Model CreatePublicModel(Guid modelId, Guid authorId, int views)
    {
        return new Model
        {
            Id = modelId,
            AuthorId = authorId,
            Privacy = PrivacySettings.Public,
            IsPublic = true,
            Views = views,
            Name = "Test"
        };
    }

    private static ClaimsPrincipal CreateUser(Guid userId)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }, "test");
        return new ClaimsPrincipal(identity);
    }
}
