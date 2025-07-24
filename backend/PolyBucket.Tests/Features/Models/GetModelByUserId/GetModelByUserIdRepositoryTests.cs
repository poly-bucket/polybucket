using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.Domain;
using PolyBucket.Api.Features.Models.GetModelByUserId.Repository;
using PolyBucket.Api.Features.Models.Domain.Enums;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.GetModelByUserId
{
    public class GetModelByUserIdRepositoryTests : IDisposable
    {
        private readonly PolyBucketDbContext _context;
        private readonly GetModelByUserIdRepository _repository;

        public GetModelByUserIdRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<PolyBucketDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new PolyBucketDbContext(options);
            _repository = new GetModelByUserIdRepository(_context);
        }

        [Fact]
        public async Task GetModelsByUserIdAsync_WithExistingModels_ShouldReturnModels()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var models = new List<Model>
            {
                new Model { Id = Guid.NewGuid(), Name = "Model 1", AuthorId = userId },
                new Model { Id = Guid.NewGuid(), Name = "Model 2", AuthorId = userId },
                new Model { Id = Guid.NewGuid(), Name = "Model 3", AuthorId = userId }
            };

            _context.Models.AddRange(models);
            await _context.SaveChangesAsync();

            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _repository.GetModelsByUserIdAsync(userId, 1, 10, false, false, cancellationToken);

            // Assert
            result.Models.ShouldNotBeNull();
            result.Models.Count().ShouldBe(3);
            result.TotalCount.ShouldBe(3);
        }

        [Fact]
        public async Task GetModelsByUserIdAsync_WithPagination_ShouldReturnCorrectPage()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var models = new List<Model>();
            for (int i = 1; i <= 15; i++)
            {
                models.Add(new Model { Id = Guid.NewGuid(), Name = $"Model {i}", AuthorId = userId });
            }

            _context.Models.AddRange(models);
            await _context.SaveChangesAsync();

            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _repository.GetModelsByUserIdAsync(userId, 2, 5, false, false, cancellationToken);

            // Assert
            result.Models.ShouldNotBeNull();
            result.Models.Count().ShouldBe(5);
            result.TotalCount.ShouldBe(15);
        }

        [Fact]
        public async Task GetModelsByUserIdAsync_WithIncludePrivate_ShouldReturnPrivateModels()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var models = new List<Model>
            {
                new Model { Id = Guid.NewGuid(), Name = "Public Model", AuthorId = userId, Privacy = PrivacySettings.Public },
                new Model { Id = Guid.NewGuid(), Name = "Private Model", AuthorId = userId, Privacy = PrivacySettings.Private }
            };

            _context.Models.AddRange(models);
            await _context.SaveChangesAsync();

            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _repository.GetModelsByUserIdAsync(userId, 1, 10, true, false, cancellationToken);

            // Assert
            result.Models.ShouldNotBeNull();
            result.Models.Count().ShouldBe(2);
        }

        [Fact]
        public async Task GetModelsByUserIdAsync_WithoutIncludePrivate_ShouldNotReturnPrivateModels()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var models = new List<Model>
            {
                new Model { Id = Guid.NewGuid(), Name = "Public Model", AuthorId = userId, Privacy = PrivacySettings.Public },
                new Model { Id = Guid.NewGuid(), Name = "Private Model", AuthorId = userId, Privacy = PrivacySettings.Private }
            };

            _context.Models.AddRange(models);
            await _context.SaveChangesAsync();

            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _repository.GetModelsByUserIdAsync(userId, 1, 10, false, false, cancellationToken);

            // Assert
            result.Models.ShouldNotBeNull();
            result.Models.Count().ShouldBe(1);
            result.Models.First().Name.ShouldBe("Public Model");
        }

        [Fact]
        public async Task GetModelsByUserIdAsync_WithIncludeDeleted_ShouldReturnDeletedModels()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var models = new List<Model>
            {
                new Model { Id = Guid.NewGuid(), Name = "Active Model", AuthorId = userId },
                new Model { Id = Guid.NewGuid(), Name = "Deleted Model", AuthorId = userId, DeletedAt = DateTime.UtcNow }
            };

            _context.Models.AddRange(models);
            await _context.SaveChangesAsync();

            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _repository.GetModelsByUserIdAsync(userId, 1, 10, false, true, cancellationToken);

            // Assert
            result.Models.ShouldNotBeNull();
            result.Models.Count().ShouldBe(2);
        }

        [Fact]
        public async Task GetModelsByUserIdAsync_WithoutIncludeDeleted_ShouldNotReturnDeletedModels()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var models = new List<Model>
            {
                new Model { Id = Guid.NewGuid(), Name = "Active Model", AuthorId = userId },
                new Model { Id = Guid.NewGuid(), Name = "Deleted Model", AuthorId = userId, DeletedAt = DateTime.UtcNow }
            };

            _context.Models.AddRange(models);
            await _context.SaveChangesAsync();

            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _repository.GetModelsByUserIdAsync(userId, 1, 10, false, false, cancellationToken);

            // Assert
            result.Models.ShouldNotBeNull();
            result.Models.Count().ShouldBe(1);
            result.Models.First().Name.ShouldBe("Active Model");
        }

        [Fact]
        public async Task GetModelsByUserIdAsync_WithNoModels_ShouldReturnEmptyList()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _repository.GetModelsByUserIdAsync(userId, 1, 10, false, false, cancellationToken);

            // Assert
            result.Models.ShouldNotBeNull();
            result.Models.Count().ShouldBe(0);
            result.TotalCount.ShouldBe(0);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
} 