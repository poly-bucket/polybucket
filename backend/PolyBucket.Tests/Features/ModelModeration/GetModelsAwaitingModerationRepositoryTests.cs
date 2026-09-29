using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Common.Models.Enums;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.ModelModeration.Domain;
using PolyBucket.Api.Features.ModelModeration.GetModelsAwaitingModeration.Repository;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

namespace PolyBucket.Tests.Features.ModelModeration;

public class GetModelsAwaitingModerationRepositoryTests : IDisposable
{
    private readonly PolyBucketDbContext _context;
    private readonly GetModelsAwaitingModerationRepository _repository;

    public GetModelsAwaitingModerationRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<PolyBucketDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new PolyBucketDbContext(options);
        _repository = new GetModelsAwaitingModerationRepository(_context);
    }

    [Fact]
    public async Task GetPendingAsync_ReturnsOnlyPendingModels()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        _context.Users.Add(new User
        {
            Id = authorId,
            Email = "author@test.com",
            Username = "author",
            PasswordHash = "x",
            Salt = "x"
        });

        var pendingModel = new Model
        {
            Id = Guid.NewGuid(),
            Name = "Pending",
            Description = "Pending model",
            AuthorId = authorId,
            Privacy = PrivacySettings.Public,
            IsPublic = false
        };
        var approvedModel = new Model
        {
            Id = Guid.NewGuid(),
            Name = "Approved",
            Description = "Approved model",
            AuthorId = authorId,
            Privacy = PrivacySettings.Public,
            IsPublic = true
        };

        _context.Models.AddRange(pendingModel, approvedModel);
        _context.ModelModerationRecords.AddRange(
            new ModelModerationRecord { Id = Guid.NewGuid(), ModelId = pendingModel.Id, Status = ModelModerationStatus.Pending },
            new ModelModerationRecord { Id = Guid.NewGuid(), ModelId = approvedModel.Id, Status = ModelModerationStatus.Approved });
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetPendingAsync(1, 20);

        // Assert
        result.TotalCount.ShouldBe(1);
        result.Items.Count.ShouldBe(1);
        result.Items[0].Name.ShouldBe("Pending");
        result.Items[0].UserName.ShouldBe("author");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
