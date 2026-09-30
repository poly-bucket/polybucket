using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Api.Features.Users.BanUser.Domain;
using PolyBucket.Api.Features.Users.GetBannedUsers.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Users.BanUser;

[Collection("TestCollection")]
public class BanUserIntegrationTests : BaseIntegrationTest
{
    public BanUserIntegrationTests(TestCollectionFixture testFixture) : base(testFixture)
    {
    }

    [Fact(DisplayName = "When an admin bans a user, BannedByUserId is stored and appears in banned users list.")]
    public async Task BanUser_PersistsBannedByUserId()
    {
        // Arrange
        await ResetStateAsync();
        var moderator = await UserFactory.CreateTestUser();
        var target = await UserFactory.CreateTestUser();
        var banService = ServiceScope.ServiceProvider.GetRequiredService<IBanUserService>();
        var listService = ServiceScope.ServiceProvider.GetRequiredService<IGetBannedUsersService>();

        // Act
        await banService.BanUserAsync(target.Id, moderator.Id, "policy", null, CancellationToken.None);

        // Assert
        var reloaded = await DbContext.Users.AsNoTracking().SingleAsync(u => u.Id == target.Id);
        reloaded.IsBanned.ShouldBeTrue();
        reloaded.BannedByUserId.ShouldBe(moderator.Id);

        var list = await listService.GetBannedUsersAsync(1, 50, CancellationToken.None);
        list.Users.ShouldContain(u => u.Id == target.Id && u.BannedByUsername == moderator.Username);
    }
}
