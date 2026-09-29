using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Common.Models.Enums;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.LikeModel.Domain;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Users.GetUserLikedModels.Domain;
using PolyBucket.Api.Features.Users.GetUserLikedModels.Repository;
using PolyBucket.Api.Features.ACL.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Users.GetUserLikedModels;

public class GetUserLikedModelsRepositoryTests : IDisposable
{
    private readonly PolyBucketDbContext _context;
    private readonly GetUserLikedModelsRepository _repository;

    public GetUserLikedModelsRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<PolyBucketDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new PolyBucketDbContext(options);
        _repository = new GetUserLikedModelsRepository(_context);
    }

    [Fact(DisplayName = "When a user has liked public models, the get user liked models repository returns those models.")]
    public async Task GetUserLikedModelsAsync_ReturnsLikedPublicModels()
    {
        var likerId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        _context.Roles.Add(new Role { Id = roleId, Name = "User", IsActive = true });
        _context.Users.Add(new User
        {
            Id = likerId,
            Username = "liker",
            Email = "liker@test.com",
            RoleId = roleId,
            PasswordHash = "hash",
            Salt = "salt"
        });
        _context.Users.Add(new User
        {
            Id = authorId,
            Username = "author",
            Email = "author@test.com",
            RoleId = roleId,
            PasswordHash = "hash",
            Salt = "salt"
        });

        var modelId = Guid.NewGuid();
        _context.Models.Add(new Model
        {
            Id = modelId,
            Name = "Liked Model",
            Description = "Description",
            AuthorId = authorId,
            Privacy = PrivacySettings.Public,
            Likes = 1,
            CreatedAt = DateTime.UtcNow,
            CreatedById = authorId,
            UpdatedById = authorId
        });
        _context.Likes.Add(new Like
        {
            Id = Guid.NewGuid(),
            ModelId = modelId,
            UserId = likerId,
            CreatedAt = DateTime.UtcNow,
            CreatedById = likerId,
            UpdatedById = likerId
        });
        await _context.SaveChangesAsync();

        var result = await _repository.GetUserLikedModelsAsync(new GetUserLikedModelsQuery
        {
            UserId = likerId,
            Page = 1,
            PageSize = 10
        }, CancellationToken.None);

        result.Models.Count().ShouldBe(1);
        result.Models.First().Name.ShouldBe("Liked Model");
        result.TotalCount.ShouldBe(1);
    }

    [Fact(DisplayName = "When a user liked a private model, the get user liked models repository excludes it.")]
    public async Task GetUserLikedModelsAsync_ExcludesPrivateModels()
    {
        var likerId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        _context.Roles.Add(new Role { Id = roleId, Name = "User", IsActive = true });
        _context.Users.Add(new User
        {
            Id = likerId,
            Username = "liker2",
            Email = "liker2@test.com",
            RoleId = roleId,
            PasswordHash = "hash",
            Salt = "salt"
        });
        _context.Users.Add(new User
        {
            Id = authorId,
            Username = "author2",
            Email = "author2@test.com",
            RoleId = roleId,
            PasswordHash = "hash",
            Salt = "salt"
        });

        var modelId = Guid.NewGuid();
        _context.Models.Add(new Model
        {
            Id = modelId,
            Name = "Private Liked",
            Description = "Description",
            AuthorId = authorId,
            Privacy = PrivacySettings.Private,
            CreatedAt = DateTime.UtcNow,
            CreatedById = authorId,
            UpdatedById = authorId
        });
        _context.Likes.Add(new Like
        {
            Id = Guid.NewGuid(),
            ModelId = modelId,
            UserId = likerId,
            CreatedAt = DateTime.UtcNow,
            CreatedById = likerId,
            UpdatedById = likerId
        });
        await _context.SaveChangesAsync();

        var result = await _repository.GetUserLikedModelsAsync(new GetUserLikedModelsQuery
        {
            UserId = likerId,
            Page = 1,
            PageSize = 10
        }, CancellationToken.None);

        result.Models.ShouldBeEmpty();
        result.TotalCount.ShouldBe(0);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
