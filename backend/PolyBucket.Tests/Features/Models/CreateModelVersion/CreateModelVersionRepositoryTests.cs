using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.CreateModel.Domain;
using PolyBucket.Api.Features.Models.CreateModelVersion.Domain;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Models.CreateModelVersion.Repository;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.CreateModelVersion
{
    public class CreateModelVersionRepositoryTests : IDisposable
    {
        private readonly PolyBucketDbContext _context;
        private readonly CreateModelVersionRepository _repository;

        public CreateModelVersionRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<PolyBucketDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new PolyBucketDbContext(options);
            _repository = new CreateModelVersionRepository(_context);
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
        public async Task CreateModelVersionAsync_WithValidVersion_ShouldSaveToDatabase()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var modelVersion = new ModelVersion
            {
                Id = Guid.NewGuid(),
                Name = "Version 2.0",
                Notes = "Updated version",
                ModelId = modelId,
                VersionNumber = 2
            };
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _repository.CreateModelVersionAsync(modelVersion, cancellationToken);

            // Assert
            result.ShouldNotBeNull();
            result.Id.ShouldBe(modelVersion.Id);
            result.Name.ShouldBe(modelVersion.Name);

            // Verify it was actually saved to the database
            var savedVersion = await _context.ModelVersions.FindAsync(modelVersion.Id);
            savedVersion.ShouldNotBeNull();
            savedVersion.Name.ShouldBe("Version 2.0");
        }

        [Fact]
        public async Task CreateModelVersionAsync_WithVersionFiles_ShouldSaveFilesToo()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var versionId = Guid.NewGuid();
            var modelVersion = new ModelVersion
            {
                Id = versionId,
                Name = "Version 2.0",
                ModelId = modelId,
                VersionNumber = 2,
                Files = new List<ModelFile>
                {
                    new ModelFile
                    {
                        Id = Guid.NewGuid(),
                        Name = "test.stl",
                        Path = "https://storage.example.com/test.stl",
                        Size = 1024,
                        MimeType = "application/octet-stream"
                    }
                }
            };
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _repository.CreateModelVersionAsync(modelVersion, cancellationToken);

            // Assert
            result.ShouldNotBeNull();
            result.Files.ShouldNotBeNull();
            result.Files.ShouldHaveSingleItem();

            // Verify files were saved
            var savedVersion = await _context.ModelVersions
                .Include(v => v.Files)
                .FirstOrDefaultAsync(v => v.Id == versionId);
            savedVersion.ShouldNotBeNull();
            savedVersion.Files.ShouldHaveSingleItem();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
} 