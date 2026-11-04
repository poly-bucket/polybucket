using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.Collections.DeleteAllCollections.Repository;
using PolyBucket.Database.Entities.Collections;
using PolyBucket.Database.Entities.Comments;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Persistence;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Collections.DeleteAllCollections;

public class DeleteAllCollectionsRepositoryTests
{
    [Fact(DisplayName = "When deleting all collections and there are none, the repository returns zero.")]
    public async Task DeleteAllCollectionsAndReturnCountAsync_WhenNone_ReturnsZero()
    {
        var options = new DbContextOptionsBuilder<PolyBucketDbContext>()
            .UseInMemoryDatabase("delete_all_collections_empty_" + Guid.NewGuid())
            .Options;
        await using var ctx = new PolyBucketDbContext(options);
        var sut = new DeleteAllCollectionsRepository(ctx);

        var count = await sut.DeleteAllCollectionsAndReturnCountAsync(CancellationToken.None);

        Assert.Equal(0, count);
    }

    [Fact(DisplayName = "When collections exist, the repository removes collections, links, and collection comments.")]
    public async Task DeleteAllCollectionsAndReturnCountAsync_WhenCollectionsExist_RemovesAll()
    {
        var options = new DbContextOptionsBuilder<PolyBucketDbContext>()
            .UseInMemoryDatabase("delete_all_collections_seeded_" + Guid.NewGuid())
            .Options;
        await using var ctx = new PolyBucketDbContext(options);

        var ownerId = Guid.NewGuid();
        var author = new User
        {
            Id = ownerId,
            Username = "owner",
            Email = "o@test.com",
            Salt = "salt",
            PasswordHash = "hash"
        };
        var collection = new Collection { Id = Guid.NewGuid(), Name = "C", OwnerId = ownerId };
        var model = new Model
        {
            Id = Guid.NewGuid(),
            Name = "M",
            Description = "D",
            AuthorId = ownerId
        };
        ctx.Users.Add(author);
        ctx.Collections.Add(collection);
        ctx.Models.Add(model);
        await ctx.SaveChangesAsync();

        ctx.CollectionModels.Add(new CollectionModel { CollectionId = collection.Id, ModelId = model.Id });
        ctx.EnhancedComments.Add(new EnhancedComment
        {
            Id = Guid.NewGuid(),
            Content = "hi",
            AuthorId = ownerId,
            TargetId = collection.Id,
            TargetType = CommentTargetType.Collection
        });
        await ctx.SaveChangesAsync();

        var sut = new DeleteAllCollectionsRepository(ctx);

        var count = await sut.DeleteAllCollectionsAndReturnCountAsync(CancellationToken.None);

        Assert.Equal(1, count);
        Assert.Empty(await ctx.Collections.ToListAsync());
        Assert.Empty(await ctx.CollectionModels.ToListAsync());
        Assert.Empty(await ctx.EnhancedComments.ToListAsync());
    }
}
