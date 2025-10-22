using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Search.Domain;
using PolyBucket.Api.Features.Search.Repository;
using PolyBucket.Api.Features.Models.Domain;
using PolyBucket.Api.Features.Models.Domain.Enums;
using PolyBucket.Api.Features.Collections.Domain;
using PolyBucket.Api.Features.Collections.Domain.Enums;
using PolyBucket.Api.Common.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PolyBucket.Tests.Features.Search
{
    public class SearchRepositoryTests : IDisposable
    {
        private readonly PolyBucketDbContext _context;
        private readonly SearchRepository _searchRepository;

        public SearchRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<PolyBucketDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new PolyBucketDbContext(options);
            _searchRepository = new SearchRepository(_context);

            SeedTestData();
        }

        [Fact]
        public async Task SearchAsync_WithValidQuery_ReturnsResults()
        {
            // Arrange
            var query = new SearchQuery
            {
                Query = "test",
                Page = 1,
                PageSize = 10,
                Type = SearchType.All
            };

            // Act
            var result = await _searchRepository.SearchAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.TotalCount > 0);
            Assert.Equal("test", result.Query);
            Assert.Equal(SearchType.All, result.Type);
        }

        [Fact]
        public async Task SearchAsync_WithModelSearch_ReturnsModels()
        {
            // Arrange
            var query = new SearchQuery
            {
                Query = "test model",
                Page = 1,
                PageSize = 10,
                Type = SearchType.Models
            };

            // Act
            var result = await _searchRepository.SearchAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Results.Any());
            Assert.All(result.Results, item => Assert.Equal(SearchResultType.Model, item.Type));
        }

        [Fact]
        public async Task SearchAsync_WithUserSearch_ReturnsUsers()
        {
            // Arrange
            var query = new SearchQuery
            {
                Query = "testuser",
                Page = 1,
                PageSize = 10,
                Type = SearchType.Users
            };

            // Act
            var result = await _searchRepository.SearchAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Results.Any());
            Assert.All(result.Results, item => Assert.Equal(SearchResultType.User, item.Type));
        }

        [Fact]
        public async Task SearchAsync_WithCollectionSearch_ReturnsCollections()
        {
            // Arrange
            var query = new SearchQuery
            {
                Query = "test collection",
                Page = 1,
                PageSize = 10,
                Type = SearchType.Collections
            };

            // Act
            var result = await _searchRepository.SearchAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Results.Any());
            Assert.All(result.Results, item => Assert.Equal(SearchResultType.Collection, item.Type));
        }

        [Fact]
        public async Task SearchAsync_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            var query = new SearchQuery
            {
                Query = "test",
                Page = 2,
                PageSize = 5,
                Type = SearchType.All
            };

            // Act
            var result = await _searchRepository.SearchAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Page);
            Assert.Equal(5, result.PageSize);
            Assert.True(result.TotalPages >= 2);
        }

        [Fact]
        public async Task SearchAsync_WithFuzzySearch_HandlesTypos()
        {
            // Arrange
            var query = new SearchQuery
            {
                Query = "tesst", // typo in "test"
                Page = 1,
                PageSize = 10,
                Type = SearchType.All
            };

            // Act
            var result = await _searchRepository.SearchAsync(query);

            // Assert
            Assert.NotNull(result);
            // Should still find results despite the typo
            Assert.True(result.TotalCount > 0);
        }

        [Fact]
        public async Task SearchAsync_WithEmptyQuery_ReturnsEmptyResults()
        {
            // Arrange
            var query = new SearchQuery
            {
                Query = "",
                Page = 1,
                PageSize = 10,
                Type = SearchType.All
            };

            // Act
            var result = await _searchRepository.SearchAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalCount);
            Assert.Empty(result.Results);
        }

        private void SeedTestData()
        {
            // Create test users
            var user1 = new User
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                Email = "testuser@example.com",
                FirstName = "Test",
                LastName = "User",
                Bio = "Test user bio",
                PasswordHash = "hash",
                Salt = "salt",
                CreatedAt = DateTime.UtcNow,
                IsProfilePublic = true
            };

            var user2 = new User
            {
                Id = Guid.NewGuid(),
                Username = "anotheruser",
                Email = "another@example.com",
                FirstName = "Another",
                LastName = "User",
                Bio = "Another user bio",
                PasswordHash = "hash",
                Salt = "salt",
                CreatedAt = DateTime.UtcNow,
                IsProfilePublic = true
            };

            _context.Users.AddRange(user1, user2);

            // Create test models
            var model1 = new Model
            {
                Id = Guid.NewGuid(),
                Name = "Test Model",
                Description = "A test model for search testing",
                AuthorId = user1.Id,
                Author = user1,
                Privacy = PrivacySettings.Public,
                CreatedAt = DateTime.UtcNow,
                Downloads = 10,
                Likes = 5
            };

            var model2 = new Model
            {
                Id = Guid.NewGuid(),
                Name = "Another Model",
                Description = "Another test model",
                AuthorId = user2.Id,
                Author = user2,
                Privacy = PrivacySettings.Public,
                CreatedAt = DateTime.UtcNow,
                Downloads = 20,
                Likes = 8
            };

            _context.Models.AddRange(model1, model2);

            // Create test collections
            var collection1 = new Collection
            {
                Id = Guid.NewGuid(),
                Name = "Test Collection",
                Description = "A test collection for search testing",
                OwnerId = user1.Id,
                Owner = user1,
                Visibility = CollectionVisibility.Public,
                CreatedAt = DateTime.UtcNow
            };

            var collection2 = new Collection
            {
                Id = Guid.NewGuid(),
                Name = "Another Collection",
                Description = "Another test collection",
                OwnerId = user2.Id,
                Owner = user2,
                Visibility = CollectionVisibility.Public,
                CreatedAt = DateTime.UtcNow
            };

            _context.Collections.AddRange(collection1, collection2);

            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
