using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.CreateModelVersion.Domain;
using PolyBucket.Api.Features.Models.Shared.Domain;
using PolyBucket.Api.Features.Models.UpdateModelVersion.Repository;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.UpdateModelVersion
{
    public class UpdateModelVersionRepositoryTests : IDisposable
    {
        private readonly PolyBucketDbContext _context;
        private readonly UpdateModelVersionRepository _repository;

        public UpdateModelVersionRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<PolyBucketDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new PolyBucketDbContext(options);
            _repository = new UpdateModelVersionRepository(_context);
        }

        [Fact]
        public async Task GetModelVersionAsync_WithExistingVersion_ShouldReturnVersion()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var versionId = Guid.NewGuid();
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

        [Fact]
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

        [Fact]
        public async Task GetModelVersionAsync_WithWrongModelId_ShouldReturnNull()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var versionId = Guid.NewGuid();
            var wrongModelId = Guid.NewGuid();
            var modelVersion = new ModelVersion
            {
                Id = versionId,
                Name = "Test Version",
                ModelId = modelId,
                VersionNumber = 1
            };

            _context.ModelVersions.Add(modelVersion);
            await _context.SaveChangesAsync();

            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _repository.GetModelVersionAsync(wrongModelId, versionId, cancellationToken);

            // Assert
            result.ShouldBeNull();
        }

        [Fact]
        public async Task UpdateModelVersionAsync_WithValidVersion_ShouldUpdateInDatabase()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var versionId = Guid.NewGuid();
            var modelVersion = new ModelVersion
            {
                Id = versionId,
                Name = "Original Version Name",
                Notes = "Original notes",
                ModelId = modelId,
                VersionNumber = 1
            };

            _context.ModelVersions.Add(modelVersion);
            await _context.SaveChangesAsync();

            // Update the version
            modelVersion.Name = "Updated Version Name";
            modelVersion.Notes = "Updated notes";

            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _repository.UpdateModelVersionAsync(modelVersion, cancellationToken);

            // Assert
            result.ShouldNotBeNull();
            result.Name.ShouldBe("Updated Version Name");
            result.Notes.ShouldBe("Updated notes");

            // Verify it was actually updated in the database
            var updatedVersion = await _context.ModelVersions.FindAsync(versionId);
            updatedVersion.ShouldNotBeNull();
            updatedVersion.Name.ShouldBe("Updated Version Name");
            updatedVersion.Notes.ShouldBe("Updated notes");
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
} 