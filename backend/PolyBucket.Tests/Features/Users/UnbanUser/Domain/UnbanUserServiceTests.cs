using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Users.UnbanUser.Domain;
using PolyBucket.Api.Features.Users.UnbanUser.Repository;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Users.UnbanUser.Domain;

public class UnbanUserServiceTests
{
    private readonly Mock<IUnbanUserRepository> _repository = new();

    [Fact(DisplayName = "When the user is banned, unban clears ban fields.")]
    public async Task UnbanUserAsync_Valid_ClearsBan()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, IsBanned = true, BanReason = "spam" };
        _repository.Setup(r => r.FindByIdForUpdateAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        var service = new UnbanUserService(_repository.Object);

        // Act
        await service.UnbanUserAsync(userId, CancellationToken.None);

        // Assert
        user.IsBanned.ShouldBeFalse();
        user.BanReason.ShouldBeNull();
        _repository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "When the user is not banned, unban throws.")]
    public async Task UnbanUserAsync_NotBanned_Throws()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, IsBanned = false };
        _repository.Setup(r => r.FindByIdForUpdateAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        var service = new UnbanUserService(_repository.Object);

        // Act
        var act = () => service.UnbanUserAsync(userId, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<InvalidOperationException>();
    }
}
