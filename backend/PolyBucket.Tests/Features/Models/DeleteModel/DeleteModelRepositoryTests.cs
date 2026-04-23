using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Models.DeleteModel.Repository;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.DeleteModel
{
    public class DeleteModelRepositoryTests : IDisposable
    {
        private readonly PolyBucketDbContext _context;
        private readonly DeleteModelRepository _repository;

        public DeleteModelRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<PolyBucketDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new PolyBucketDbContext(options);
            _repository = new DeleteModelRepository(_context);
        }

        [Fact]
        public async Task GetModelByIdAsync_WithExistingModel_ShouldReturnModel()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var model = new Model
            {
                Id = modelId,
                Name = "Test Model",
                Description = "Test Description",
                AuthorId = Guid.NewGuid()
            };

            _context.Models.Add(model);
            await _context.SaveChangesAsync();

            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _repository.GetModelByIdAsync(modelId, cancellationToken);

            // Assert
            result.ShouldNotBeNull();
            result.Id.ShouldBe(modelId);
            result.Name.ShouldBe("Test Model");
        }

        [Fact]
        public async Task GetModelByIdAsync_WithNonExistingModel_ShouldReturnNull()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _repository.GetModelByIdAsync(modelId, cancellationToken);

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public async Task DeleteModelAsync_WithValidModel_ShouldMarkAsDeleted()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var model = new Model
            {
                Id = modelId,
                Name = "Test Model",
                Description = "Test Description",
                AuthorId = Guid.NewGuid()
            };

            _context.Models.Add(model);
            await _context.SaveChangesAsync();

            var cancellationToken = CancellationToken.None;

            // Act
            await _repository.DeleteModelAsync(model, cancellationToken);

            // Assert
            var deletedModel = await _context.Models.FindAsync(modelId);
            deletedModel.ShouldNotBeNull();
            deletedModel.DeletedAt.ShouldNotBeNull();
            deletedModel.DeletedById.ShouldBe(userId);
        }

        [Fact]
        public async Task DeleteModelAsync_ShouldUpdateUpdatedAt()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var model = new Model
            {
                Id = modelId,
                Name = "Test Model",
                AuthorId = Guid.NewGuid()
            };

            _context.Models.Add(model);
            await _context.SaveChangesAsync();

            var originalUpdatedAt = model.UpdatedAt ?? DateTime.UtcNow;
            var cancellationToken = CancellationToken.None;

            // Act
            await _repository.DeleteModelAsync(model, cancellationToken);

            // Assert
            var deletedModel = await _context.Models.FindAsync(modelId);
            deletedModel.ShouldNotBeNull();
            deletedModel.UpdatedAt.ShouldNotBeNull();
            deletedModel.UpdatedAt!.Value.ShouldBeGreaterThan(originalUpdatedAt);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
} 