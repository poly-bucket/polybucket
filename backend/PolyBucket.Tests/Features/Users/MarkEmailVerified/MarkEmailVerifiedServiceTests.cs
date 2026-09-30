using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PolyBucket.Api.Common;
using PolyBucket.Api.Common.Http;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Users.Domain;
using PolyBucket.Api.Features.Users.MarkEmailVerified.Domain;
using PolyBucket.Api.Features.Users.MarkEmailVerified.Repository;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Users.MarkEmailVerified;

public class MarkEmailVerifiedServiceTests
{
    private readonly Mock<IMarkEmailVerifiedRepository> _repository = new();
    private readonly Guid _adminId = Guid.NewGuid();

    private MarkEmailVerifiedService CreateService() =>
        new(_repository.Object, TimeProvider.System, NullLogger<MarkEmailVerifiedService>.Instance);

    [Fact(DisplayName = "When an admin marks an unverified user, the user is verified and an audit entry is written.")]
    public async Task MarkEmailVerified_UnverifiedUser_ShouldVerifyAndAudit()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "user@example.com", Username = "user" };
        _repository.Setup(r => r.FindByIdForUpdateAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        // Act
        var verifiedAt = await CreateService().MarkEmailVerifiedAsync(user.Id, _adminId, new ClientRequestInfo("10.0.0.1", "tests"));

        // Assert
        user.EmailVerifiedAt.ShouldBe(verifiedAt);
        _repository.Verify(r => r.AddAuditLog(It.Is<UserAuditLog>(a =>
            a.UserId == user.Id
            && a.PerformedByUserId == _adminId
            && a.Action == UserAuditAction.EmailMarkedVerified
            && a.IpAddress == "10.0.0.1")), Times.Once);
        _repository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "When the user is already verified, the original verification time is kept and nothing is written.")]
    public async Task MarkEmailVerified_AlreadyVerified_ShouldBeNoOp()
    {
        // Arrange
        var original = DateTime.UtcNow.AddDays(-3);
        var user = new User { Id = Guid.NewGuid(), Email = "user@example.com", Username = "user", EmailVerifiedAt = original };
        _repository.Setup(r => r.FindByIdForUpdateAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        // Act
        var verifiedAt = await CreateService().MarkEmailVerifiedAsync(user.Id, _adminId, ClientRequestInfo.Unknown);

        // Assert
        verifiedAt.ShouldBe(original);
        _repository.Verify(r => r.AddAuditLog(It.IsAny<UserAuditLog>()), Times.Never);
        _repository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "When the user does not exist, the service throws NotFoundException.")]
    public async Task MarkEmailVerified_UnknownUser_ShouldThrowNotFound()
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = () => service.MarkEmailVerifiedAsync(Guid.NewGuid(), _adminId, ClientRequestInfo.Unknown);

        // Assert
        await Should.ThrowAsync<NotFoundException>(act);
    }
}
