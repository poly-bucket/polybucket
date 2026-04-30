using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.CreateModelVersion.Domain;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Models.DeleteModelVersion.Repository;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.DeleteModelVersion
{
    public class DeleteModelVersionRepositoryTests : IDisposable
    {
        private readonly PolyBucketDbContext _context;
        private readonly DeleteModelVersionRepository _repository;

        public DeleteModelVersionRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<PolyBucketDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new PolyBucketDbContext(options);
            _repository = new DeleteModelVersionRepository(_context);
        }

        [Fact(DisplayName = "When getting a model version with an existing version, the delete model version repository returns the version.")]
        public async Task GetModelVersionAsync_WithExistingVersion_ShouldReturnVersion()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var versionId = Guid.NewGuid();
            var model = new Model
            {
                Id = modelId,
                Name = "Parent",
                Description = "Desc",
                AuthorId = Guid.NewGuid()
            };
            _context.Models.Add(model);
            var modelVersion = new ModelVersion
            {
                Id = versionId,
                Name = "Test Version",
                Notes = "Test notes",
                ModelId = modelId,
                VersionNumber = 1
            };

            _context.ModelVersions.Add(modelVersion);
            await _context.SaveChangesAsync();

            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _repository.GetModelVersionAsync(modelId, versionId, cancellationToken);

            // Assert
            result.ShouldNotBeNull();
            result.Id.ShouldBe(versionId);
            result.Name.ShouldBe("Test Version");
        }

        [Fact(DisplayName = "When getting a model version with a non-existent version, the delete model version repository returns null.")]
        public async Task GetModelVersionAsync_WithNonExistingVersion_ShouldReturnNull()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var versionId = Guid.NewGuid();
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _repository.GetModelVersionAsync(modelId, versionId, cancellationToken);

            // Assert
            result.ShouldBeNull();
        }

        [Fact(DisplayName = "When deleting a model version, the delete model version repository marks it as deleted.")]
        public async Task DeleteModelVersionAsync_WithValidVersion_ShouldMarkAsDeleted()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var versionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var model = new Model
            {
                Id = modelId,
                Name = "Parent",
                Description = "Desc",
                AuthorId = Guid.NewGuid()
            };
            _context.Models.Add(model);
            var modelVersion = new ModelVersion
            {
                Id = versionId,
                Name = "Test Version",
                Notes = "Test notes",
                ModelId = modelId,
                VersionNumber = 1
            };

            _context.ModelVersions.Add(modelVersion);
            await _context.SaveChangesAsync();

            var cancellationToken = CancellationToken.None;
            var now = DateTime.UtcNow;
            modelVersion.DeletedAt = now;
            modelVersion.DeletedById = userId;
            modelVersion.UpdatedAt = now;
            modelVersion.UpdatedById = userId;

            // Act
            await _repository.DeleteModelVersionAsync(modelVersion, cancellationToken);

            // Assert
            var deletedVersion = await _context.ModelVersions.FindAsync(versionId);
            deletedVersion.ShouldNotBeNull();
            deletedVersion.DeletedAt.ShouldNotBeNull();
            deletedVersion.DeletedById.ShouldBe(userId);
        }

        [Fact(DisplayName = "When deleting a model version, the delete model version repository updates the UpdatedAt timestamp.")]
        public async Task DeleteModelVersionAsync_ShouldUpdateUpdatedAt()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var versionId = Guid.NewGuid();
            var authorId = Guid.NewGuid();
            var model = new Model
            {
                Id = modelId,
                Name = "Parent",
                Description = "Desc",
                AuthorId = authorId
            };
            _context.Models.Add(model);
            var modelVersion = new ModelVersion
            {
                Id = versionId,
                Name = "Test Version",
                Notes = "Notes",
                ModelId = modelId,
                VersionNumber = 1
            };

            _context.ModelVersions.Add(modelVersion);
            await _context.SaveChangesAsync();

            var originalUpdatedAt = DateTime.UtcNow.AddHours(-1);
            var cancellationToken = CancellationToken.None;
            var now = DateTime.UtcNow;
            modelVersion.DeletedAt = now;
            modelVersion.DeletedById = authorId;
            modelVersion.UpdatedAt = now;
            modelVersion.UpdatedById = authorId;

            // Act
            await _repository.DeleteModelVersionAsync(modelVersion, cancellationToken);

            // Assert
            var deletedVersion = await _context.ModelVersions.FindAsync(versionId);
            deletedVersion.ShouldNotBeNull();
            deletedVersion.UpdatedAt.ShouldNotBeNull();
            deletedVersion.UpdatedAt!.Value.ShouldBeGreaterThan(originalUpdatedAt);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
} 