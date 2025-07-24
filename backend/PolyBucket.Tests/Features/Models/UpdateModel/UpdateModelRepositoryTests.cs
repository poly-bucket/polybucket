using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.Domain;
using PolyBucket.Api.Features.Models.UpdateModel.Repository;
using Shouldly;
using Xunit;
using System.Collections.Generic;

namespace PolyBucket.Tests.Features.Models.UpdateModel
{
    public class UpdateModelRepositoryTests : IDisposable
    {
        private readonly PolyBucketDbContext _context;
        private readonly UpdateModelRepository _repository;

        public UpdateModelRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<PolyBucketDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new PolyBucketDbContext(options);
            _repository = new UpdateModelRepository(_context);
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
        public async Task GetModelByIdAsync_ShouldIncludeFiles()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var model = new Model
            {
                Id = modelId,
                Name = "Test Model",
                AuthorId = Guid.NewGuid(),
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

            _context.Models.Add(model);
            await _context.SaveChangesAsync();

            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _repository.GetModelByIdAsync(modelId, cancellationToken);

            // Assert
            result.ShouldNotBeNull();
            result.Files.ShouldNotBeNull();
            result.Files.ShouldHaveSingleItem();
        }

        [Fact]
        public async Task UpdateModelAsync_WithValidModel_ShouldUpdateInDatabase()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var model = new Model
            {
                Id = modelId,
                Name = "Original Name",
                Description = "Original Description",
                AuthorId = Guid.NewGuid()
            };

            _context.Models.Add(model);
            await _context.SaveChangesAsync();

            // Update the model
            model.Name = "Updated Name";
            model.Description = "Updated Description";

            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _repository.UpdateModelAsync(model, cancellationToken);

            // Assert
            result.ShouldNotBeNull();
            result.Name.ShouldBe("Updated Name");
            result.Description.ShouldBe("Updated Description");

            // Verify it was actually updated in the database
            var updatedModel = await _context.Models.FindAsync(modelId);
            updatedModel.ShouldNotBeNull();
            updatedModel.Name.ShouldBe("Updated Name");
            updatedModel.Description.ShouldBe("Updated Description");
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
} 