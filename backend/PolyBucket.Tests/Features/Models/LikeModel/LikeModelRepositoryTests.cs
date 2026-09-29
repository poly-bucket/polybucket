using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Common.Models.Enums;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.LikeModel.Domain;
using PolyBucket.Api.Features.Models.LikeModel.Repository;
using PolyBucket.Api.Features.SystemSettings.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.LikeModel;

public class LikeModelRepositoryTests : IDisposable
{
    private readonly PolyBucketDbContext _context;
    private readonly LikeModelRepository _repository;

    public LikeModelRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<PolyBucketDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new PolyBucketDbContext(options);
        _repository = new LikeModelRepository(_context);
    }

    [Fact(DisplayName = "When model likes are enabled in settings, the like model repository reports likes as enabled.")]
    public async Task IsModelLikesEnabledAsync_WhenEnabled_ReturnsTrue()
    {
        _context.ModelSettings.Add(new ModelSettings
        {
            Id = Guid.NewGuid(),
            EnableModelLikes = true
        });
        await _context.SaveChangesAsync();

        var result = await _repository.IsModelLikesEnabledAsync(CancellationToken.None);

        result.ShouldBeTrue();
    }

    [Fact(DisplayName = "When saving a new like through the repository, the like is persisted.")]
    public async Task AddLike_PersistsLikeRecord()
    {
        var modelId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _context.Models.Add(new Model
        {
            Id = modelId,
            Name = "Model",
            Description = "Desc",
            AuthorId = userId,
            Privacy = PrivacySettings.Public,
            CreatedAt = DateTime.UtcNow,
            CreatedById = userId,
            UpdatedById = userId
        });
        await _context.SaveChangesAsync();

        var like = new Like
        {
            Id = Guid.NewGuid(),
            ModelId = modelId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            CreatedById = userId,
            UpdatedById = userId
        };

        _repository.AddLike(like);
        await _repository.SaveChangesAsync(CancellationToken.None);

        var stored = await _context.Likes.FirstOrDefaultAsync(l => l.ModelId == modelId && l.UserId == userId);
        stored.ShouldNotBeNull();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
