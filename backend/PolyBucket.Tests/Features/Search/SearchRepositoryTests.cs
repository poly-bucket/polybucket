using Microsoft.EntityFrameworkCore;
using Moq;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Search.Domain;
using PolyBucket.Api.Features.Search.Repository;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Common.Models.Enums;
using PolyBucket.Api.Features.Collections.Domain;
using PolyBucket.Api.Features.Collections.Domain.Enums;
using System;
using System.Linq;
using System.Threading;
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
            var capabilities = new Mock<ISearchCapabilities>();
            capabilities.Setup(c => c.GetModeAsync(It.IsAny<PolyBucketDbContext>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(SearchTextMode.Basic);
            _searchRepository = new SearchRepository(_context, capabilities.Object);

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
                PageSize = 1,
                Type = SearchType.Models
            };

            // Act
            var result = await _searchRepository.SearchAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Page);
            Assert.Equal(1, result.PageSize);
            Assert.Equal(2, result.TotalPages);
            Assert.Single(result.Results);
        }

        [Fact]
        public async Task SearchAsync_WithAllType_ReportsCountsPerType()
        {
            // Arrange
            var query = new SearchQuery
            {
                Query = "test",
                Page = 1,
                PageSize = 1,
                Type = SearchType.All
            };

            // Act
            var result = await _searchRepository.SearchAsync(query);

            // Assert
            Assert.Equal(2, result.Counts.Models);
            Assert.Equal(2, result.Counts.Users);
            Assert.Equal(2, result.Counts.Collections);
            Assert.Equal(6, result.TotalCount);
            Assert.Equal(3, result.Results.Count());
            Assert.Equal(2, result.TotalPages);
        }

        [Fact]
        public async Task SearchAsync_WithoutTrigramSupport_DoesNotMatchTypos()
        {
            // Arrange
            var query = new SearchQuery
            {
                Query = "tesst",
                Page = 1,
                PageSize = 10,
                Type = SearchType.All
            };

            // Act
            var result = await _searchRepository.SearchAsync(query);

            // Assert
            Assert.Equal(0, result.TotalCount);
        }

        [Fact]
        public async Task SearchAsync_WithPrivateOrDeletedModels_ExcludesThem()
        {
            // Arrange
            var author = _context.Users.First();
            _context.Models.AddRange(
                new Model { Id = Guid.NewGuid(), Name = "Hidden test", AuthorId = author.Id, Privacy = PrivacySettings.Private, IsPublic = false, CreatedAt = DateTime.UtcNow },
                new Model { Id = Guid.NewGuid(), Name = "Deleted test", AuthorId = author.Id, Privacy = PrivacySettings.Public, IsPublic = true, DeletedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow });
            await _context.SaveChangesAsync();
            var query = new SearchQuery { Query = "test", Page = 1, PageSize = 10, Type = SearchType.Models };

            // Act
            var result = await _searchRepository.SearchAsync(query);

            // Assert
            Assert.DoesNotContain(result.Results, r => r.Title == "Hidden test" || r.Title == "Deleted test");
        }

        [Fact]
        public async Task SearchAsync_WithUserSearch_HidesEmailUnlessShown()
        {
            // Arrange
            var query = new SearchQuery { Query = "user", Page = 1, PageSize = 10, Type = SearchType.Users };

            // Act
            var result = await _searchRepository.SearchAsync(query);

            // Assert
            Assert.All(result.Results, r => Assert.Null(r.Email));
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
                Bio = "Another test user bio",
                PasswordHash = "hash",
                Salt = "salt",
                CreatedAt = DateTime.UtcNow,
                IsProfilePublic = true
            };

            _context.Users.AddRange(user1, user2);

            var model1 = new Model
            {
                Id = Guid.NewGuid(),
                Name = "Test Model",
                Description = "A test model for search testing",
                AuthorId = user1.Id,
                Author = user1,
                Privacy = PrivacySettings.Public,
                IsPublic = true,
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
                IsPublic = true,
                CreatedAt = DateTime.UtcNow,
                Downloads = 20,
                Likes = 8
            };

            _context.Models.AddRange(model1, model2);

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
