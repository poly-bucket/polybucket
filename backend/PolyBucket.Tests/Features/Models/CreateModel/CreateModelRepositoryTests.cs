using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.Domain;
using PolyBucket.Api.Features.Models.CreateModel.Repository;
using Shouldly;
using Xunit;
using System.Collections.Generic;
using System.Linq;

namespace PolyBucket.Tests.Features.Models.CreateModel
{
    public class CreateModelRepositoryTests : IDisposable
    {
        private readonly PolyBucketDbContext _context;
        private readonly CreateModelRepository _repository;

        public CreateModelRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<PolyBucketDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new PolyBucketDbContext(options);
            _repository = new CreateModelRepository(_context);
        }

        [Fact]
        public async Task CreateModelAsync_WithValidModel_ShouldSaveToDatabase()
        {
            // Arrange
            var model = new Model
            {
                Id = Guid.NewGuid(),
                Name = "Test Model",
                Description = "Test Description",
                AuthorId = Guid.NewGuid()
            };
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _repository.CreateModelAsync(model, cancellationToken);

            // Assert
            result.ShouldNotBeNull();
            result.Id.ShouldBe(model.Id);
            result.Name.ShouldBe(model.Name);

            // Verify it was actually saved to the database
            var savedModel = await _context.Models.FindAsync(model.Id);
            savedModel.ShouldNotBeNull();
            savedModel.Name.ShouldBe("Test Model");
        }

        [Fact]
        public async Task CreateModelAsync_WithModelFiles_ShouldSaveFilesToo()
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
            var cancellationToken = CancellationToken.None;

            // Act
            var result = await _repository.CreateModelAsync(model, cancellationToken);

            // Assert
            result.ShouldNotBeNull();
            result.Files.ShouldNotBeNull();
            result.Files.ShouldHaveSingleItem();

            // Verify files were saved
            var savedModel = await _context.Models
                .Include(m => m.Files)
                .FirstOrDefaultAsync(m => m.Id == modelId);
            savedModel.ShouldNotBeNull();
            savedModel.Files.ShouldHaveSingleItem();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
} 