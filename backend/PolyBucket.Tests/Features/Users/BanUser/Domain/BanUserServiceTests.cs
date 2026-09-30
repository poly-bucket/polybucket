using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Users.BanUser.Domain;
using PolyBucket.Api.Features.Users.BanUser.Repository;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Users.BanUser.Domain;

public class BanUserServiceTests
{
    private readonly Mock<IBanUserRepository> _repository = new();

    [Fact(DisplayName = "When the user exists and is not banned, ban persists the ban.")]
    public async Task BanUserAsync_Valid_BansUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var user = new User { Id = userId, IsBanned = false };
        _repository.Setup(r => r.FindByIdForUpdateAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        var service = new BanUserService(_repository.Object);

        // Act
        await service.BanUserAsync(userId, adminId, "spam", null, CancellationToken.None);

        // Assert
        user.IsBanned.ShouldBeTrue();
        user.BanReason.ShouldBe("spam");
        _repository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "When the user is missing, ban throws.")]
    public async Task BanUserAsync_MissingUser_Throws()
    {
        // Arrange
        _repository.Setup(r => r.FindByIdForUpdateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        var service = new BanUserService(_repository.Object);

        // Act
        var act = () => service.BanUserAsync(Guid.NewGuid(), Guid.NewGuid(), "spam", null, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<InvalidOperationException>();
    }
}
