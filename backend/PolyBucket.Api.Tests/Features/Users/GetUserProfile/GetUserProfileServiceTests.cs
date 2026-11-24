using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using PolyBucket.Api.Features.Users.Follow.Repository;
using PolyBucket.Api.Features.Users.GetUserProfile.Domain;
using PolyBucket.Api.Features.Users.GetUserProfile.Repository;
using PolyBucket.Database.Entities.ACL;
using PolyBucket.Database.Entities.Models;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Users.GetUserProfile;

public class GetUserProfileServiceTests
{
    private readonly Mock<IGetUserProfileRepository> _repository = new();
    private readonly Mock<IUserFollowRepository> _followRepository = new();

    private GetUserProfileService CreateService() =>
        new(_repository.Object, _followRepository.Object);

    private static User CreateUser(Guid id, bool isPublic = true)
    {
        return new User
        {
            Id = id,
            Username = "target",
            Email = "target@example.com",
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow.AddDays(-1),
            IsProfilePublic = isPublic,
            ShowEmail = false,
            ShowLastLogin = false,
            ShowStatistics = false,
            Role = new Role { Name = "User" }
        };
    }

    private static GetUserProfileStatisticsRow SampleStats() => new()
    {
        TotalModels = 3,
        TotalCollections = 2,
        TotalLikes = 5,
        TotalDownloads = 10,
        TotalFollowers = 1,
        TotalFollowing = 4
    };

    [Fact(DisplayName = "When a profile is private and the viewer is a stranger, GetUserProfile returns PrivateProfileResponse.")]
    public async Task GetUserProfileAsync_PrivateProfile_Stranger_ReturnsStub()
    {
        var userId = Guid.NewGuid();
        var user = CreateUser(userId, isPublic: false);
        _repository
            .Setup(r => r.GetByIdWithRoleAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var query = new GetUserProfileQuery { Id = userId };
        var result = await CreateService().GetUserProfileAsync(query, CancellationToken.None);

        var stub = Assert.IsType<PrivateProfileResponse>(result);
        Assert.Equal(userId, stub.Id);
        Assert.False(stub.IsProfilePublic);
    }

    [Fact(DisplayName = "When a profile is private and the viewer is the owner, GetUserProfile returns a full response.")]
    public async Task GetUserProfileAsync_PrivateProfile_Owner_ReturnsFullProfile()
    {
        var userId = Guid.NewGuid();
        var user = CreateUser(userId, isPublic: false);
        _repository
            .Setup(r => r.GetByIdWithRoleAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _repository
            .Setup(r => r.GetProfileStatisticsAsync(userId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(SampleStats());

        var query = new GetUserProfileQuery { Id = userId, RequestingUserId = userId };
        var result = await CreateService().GetUserProfileAsync(query, CancellationToken.None);

        var profile = Assert.IsType<GetUserProfileResponse>(result);
        Assert.Equal("target@example.com", profile.Email);
        Assert.Equal(3, profile.TotalModels);
    }

    [Fact(DisplayName = "When show statistics is off, a stranger does not receive statistic counts.")]
    public async Task GetUserProfileAsync_ShowStatisticsOff_Stranger_StatsNull()
    {
        var userId = Guid.NewGuid();
        var user = CreateUser(userId);
        user.ShowStatistics = false;
        _repository
            .Setup(r => r.GetByIdWithRoleAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _repository
            .Setup(r => r.GetProfileStatisticsAsync(userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(SampleStats());

        var result = await CreateService().GetUserProfileAsync(
            new GetUserProfileQuery { Id = userId },
            CancellationToken.None);

        var profile = Assert.IsType<GetUserProfileResponse>(result);
        Assert.Null(profile.TotalModels);
        Assert.Null(profile.ShowStatistics);
    }

    [Fact(DisplayName = "When show email is off, a stranger does not receive an email address.")]
    public async Task GetUserProfileAsync_ShowEmailOff_Stranger_EmailNull()
    {
        var userId = Guid.NewGuid();
        var user = CreateUser(userId);
        user.ShowEmail = false;
        _repository
            .Setup(r => r.GetByIdWithRoleAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _repository
            .Setup(r => r.GetProfileStatisticsAsync(userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(SampleStats());

        var result = await CreateService().GetUserProfileAsync(
            new GetUserProfileQuery { Id = userId },
            CancellationToken.None);

        var profile = Assert.IsType<GetUserProfileResponse>(result);
        Assert.Null(profile.Email);
    }

    [Fact(DisplayName = "When show email is off, the owner still receives an email address.")]
    public async Task GetUserProfileAsync_ShowEmailOff_Owner_EmailPresent()
    {
        var userId = Guid.NewGuid();
        var user = CreateUser(userId);
        user.ShowEmail = false;
        _repository
            .Setup(r => r.GetByIdWithRoleAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _repository
            .Setup(r => r.GetProfileStatisticsAsync(userId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(SampleStats());

        var result = await CreateService().GetUserProfileAsync(
            new GetUserProfileQuery { Id = userId, RequestingUserId = userId },
            CancellationToken.None);

        var profile = Assert.IsType<GetUserProfileResponse>(result);
        Assert.Equal("target@example.com", profile.Email);
        Assert.False(profile.ShowEmail);
    }

    [Fact(DisplayName = "When show last login is off, a stranger does not receive last login.")]
    public async Task GetUserProfileAsync_ShowLastLoginOff_Stranger_LastLoginNull()
    {
        var userId = Guid.NewGuid();
        var user = CreateUser(userId);
        user.ShowLastLogin = false;
        _repository
            .Setup(r => r.GetByIdWithRoleAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _repository
            .Setup(r => r.GetProfileStatisticsAsync(userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(SampleStats());

        var result = await CreateService().GetUserProfileAsync(
            new GetUserProfileQuery { Id = userId },
            CancellationToken.None);

        var profile = Assert.IsType<GetUserProfileResponse>(result);
        Assert.Null(profile.LastLoginAt);
    }

    [Fact(DisplayName = "When an admin views a private profile, GetUserProfile returns a full response.")]
    public async Task GetUserProfileAsync_PrivateProfile_Admin_ReturnsFullProfile()
    {
        var userId = Guid.NewGuid();
        var user = CreateUser(userId, isPublic: false);
        _repository
            .Setup(r => r.GetByIdWithRoleAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _repository
            .Setup(r => r.GetProfileStatisticsAsync(userId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(SampleStats());

        var result = await CreateService().GetUserProfileAsync(
            new GetUserProfileQuery { Id = userId, IsRequestingUserAdmin = true },
            CancellationToken.None);

        Assert.IsType<GetUserProfileResponse>(result);
    }
}