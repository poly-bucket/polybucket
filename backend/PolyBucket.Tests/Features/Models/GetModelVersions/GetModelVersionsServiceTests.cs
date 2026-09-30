using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using PolyBucket.Api.Common.Models.Enums;
using PolyBucket.Api.Common.Storage;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.Models.CreateModel.Domain;
using PolyBucket.Api.Features.Models.CreateModelVersion.Domain;
using PolyBucket.Api.Features.Models.GetModelVersions.Domain;
using PolyBucket.Api.Features.Models.GetModelVersions.Repository;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.GetModelVersions;

public class GetModelVersionsServiceTests
{
    private static readonly Guid ModelId = Guid.NewGuid();
    private static readonly Guid AuthorId = Guid.NewGuid();

    private readonly Mock<IGetModelVersionsRepository> _repository = new();
    private readonly Mock<IPermissionService> _permissions = new();
    private readonly Mock<IStorageService> _storage = new();
    private readonly GetModelVersionsService _service;

    public GetModelVersionsServiceTests()
    {
        _storage
            .Setup(s => s.GetPresignedUrlAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string key, TimeSpan _, CancellationToken _) => $"https://storage.test/{key}");
        _repository
            .Setup(r => r.GetVersionsAsync(ModelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new List<ModelVersion>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    ModelId = ModelId,
                    Name = "v2",
                    VersionNumber = 2,
                    FileUrl = "models/v2.stl",
                    Files = new List<ModelFile> { new() { Id = Guid.NewGuid(), Name = "v2.stl", Path = "models/v2/v2.stl" } }
                },
                new() { Id = Guid.NewGuid(), ModelId = ModelId, Name = "v1", VersionNumber = 1 }
            });
        _service = new GetModelVersionsService(_repository.Object, _permissions.Object, _storage.Object);
    }

    private void GivenAccess(PrivacySettings privacy, bool isPublic = true, bool passedModeration = true)
    {
        _repository
            .Setup(r => r.GetAccessInfoAsync(ModelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ModelVersionsAccessInfo(AuthorId, privacy, isPublic, passedModeration));
    }

    [Fact(DisplayName = "When a public approved model is requested anonymously, its versions are returned newest first with presigned file URLs.")]
    public async Task PublicModel_Anonymous_ReturnsVersions()
    {
        // Arrange
        GivenAccess(PrivacySettings.Public);

        // Act
        var result = await _service.GetModelVersionsAsync(ModelId, null, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ModelId.ShouldBe(ModelId);
        result.Versions.Count.ShouldBe(2);
        result.Versions[0].Name.ShouldBe("v2");
        result.Versions[0].FileUrl.ShouldBe("https://storage.test/models/v2.stl");
        result.Versions[0].Files[0].Path.ShouldBe("https://storage.test/models/v2/v2.stl");
    }

    [Fact(DisplayName = "When the model does not exist, nothing is returned and versions are not loaded.")]
    public async Task MissingModel_ReturnsNull()
    {
        // Arrange
        _repository
            .Setup(r => r.GetAccessInfoAsync(ModelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ModelVersionsAccessInfo?)null);

        // Act
        var result = await _service.GetModelVersionsAsync(ModelId, Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.ShouldBeNull();
        _repository.Verify(r => r.GetVersionsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "When a private model is requested by another user, nothing is returned.")]
    public async Task PrivateModel_OtherUser_ReturnsNull()
    {
        // Arrange
        GivenAccess(PrivacySettings.Private);

        // Act
        var result = await _service.GetModelVersionsAsync(ModelId, Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    [Fact(DisplayName = "When a model is pending moderation, anonymous viewers and other users cannot see its versions.")]
    public async Task PendingModel_NonOwner_ReturnsNull()
    {
        // Arrange
        GivenAccess(PrivacySettings.Public, passedModeration: false);

        // Act
        var anonymous = await _service.GetModelVersionsAsync(ModelId, null, CancellationToken.None);
        var otherUser = await _service.GetModelVersionsAsync(ModelId, Guid.NewGuid(), CancellationToken.None);

        // Assert
        anonymous.ShouldBeNull();
        otherUser.ShouldBeNull();
    }

    [Fact(DisplayName = "When the owner requests their private pending model, the versions are returned.")]
    public async Task PrivatePendingModel_Owner_ReturnsVersions()
    {
        // Arrange
        GivenAccess(PrivacySettings.Private, isPublic: false, passedModeration: false);

        // Act
        var result = await _service.GetModelVersionsAsync(ModelId, AuthorId, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Versions.Count.ShouldBe(2);
    }

    [Fact(DisplayName = "When an admin requests a private model, the versions are returned.")]
    public async Task PrivateModel_Admin_ReturnsVersions()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        GivenAccess(PrivacySettings.Private);
        _permissions.Setup(p => p.IsAdminAsync(adminId)).ReturnsAsync(true);

        // Act
        var result = await _service.GetModelVersionsAsync(ModelId, adminId, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
    }

    [Fact(DisplayName = "When a moderator requests a pending model, the versions are returned.")]
    public async Task PendingModel_Moderator_ReturnsVersions()
    {
        // Arrange
        var moderatorId = Guid.NewGuid();
        GivenAccess(PrivacySettings.Public, passedModeration: false);
        _permissions.Setup(p => p.GetUserRoleAsync(moderatorId)).ReturnsAsync(new Role { Name = "Moderator" });

        // Act
        var result = await _service.GetModelVersionsAsync(ModelId, moderatorId, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
    }

    [Fact(DisplayName = "When an unlisted model is requested, signed-in users can see it but anonymous visitors cannot.")]
    public async Task UnlistedModel_RequiresSignIn()
    {
        // Arrange
        GivenAccess(PrivacySettings.Unlisted);

        // Act
        var anonymous = await _service.GetModelVersionsAsync(ModelId, null, CancellationToken.None);
        var signedIn = await _service.GetModelVersionsAsync(ModelId, Guid.NewGuid(), CancellationToken.None);

        // Assert
        anonymous.ShouldBeNull();
        signedIn.ShouldNotBeNull();
    }
}
