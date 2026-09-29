using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Users.GetUserLikedModels.Domain;
using PolyBucket.Api.Features.Users.GetUserLikedModels.Repository;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Users.GetUserLikedModels;

public class GetUserLikedModelsServiceTests : IDisposable
{
    private readonly PolyBucketDbContext _context;
    private readonly Mock<IGetUserLikedModelsRepository> _mockRepository;

    public GetUserLikedModelsServiceTests()
    {
        var options = new DbContextOptionsBuilder<PolyBucketDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new PolyBucketDbContext(options);
        _mockRepository = new Mock<IGetUserLikedModelsRepository>();
    }

    [Fact(DisplayName = "When resolving liked models by username, the get user liked models service passes the resolved user id to the repository.")]
    public async Task GetUserLikedModelsAsync_WithUsername_ResolvesUserId()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        _context.Roles.Add(new Role { Id = roleId, Name = "User", IsActive = true });
        _context.Users.Add(new User
        {
            Id = userId,
            Username = "profileuser",
            Email = "profile@test.com",
            RoleId = roleId,
            PasswordHash = "hash",
            Salt = "salt"
        });
        await _context.SaveChangesAsync();

        GetUserLikedModelsQuery? capturedQuery = null;
        _mockRepository
            .Setup(r => r.GetUserLikedModelsAsync(It.IsAny<GetUserLikedModelsQuery>(), It.IsAny<CancellationToken>()))
            .Callback<GetUserLikedModelsQuery, CancellationToken>((q, _) => capturedQuery = q)
            .ReturnsAsync(new GetUserLikedModelsResult());

        var service = new GetUserLikedModelsService(_mockRepository.Object, _context);

        await service.GetUserLikedModelsAsync(new GetUserLikedModelsQuery
        {
            Username = "profileuser",
            Page = 1,
            PageSize = 10
        });

        capturedQuery.ShouldNotBeNull();
        capturedQuery!.UserId.ShouldBe(userId);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
