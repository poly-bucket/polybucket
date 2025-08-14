using Microsoft.EntityFrameworkCore;
using Moq;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.Domain;
using PolyBucket.Api.Features.Collections.Domain;
using PolyBucket.Api.Features.Users.Services;
using PolyBucket.Api.Common.Models;
using CommentDomain = PolyBucket.Api.Features.Comments.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PolyBucket.Tests.Features.Users
{
    public class UserStatisticsServiceTests
    {
        private readonly Mock<PolyBucketDbContext> _mockDbContext;
        private readonly UserStatisticsService _service;

        public UserStatisticsServiceTests()
        {
            _mockDbContext = new Mock<PolyBucketDbContext>();
            _service = new UserStatisticsService(_mockDbContext.Object);
        }

        [Fact]
        public async Task GetUserStatisticsAsync_ValidUserId_ReturnsCorrectStatistics()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var mockModels = CreateMockModels(userId);
            var mockCollections = CreateMockCollections(userId);
            var mockLikes = CreateMockLikes(userId);
            var mockComments = CreateMockComments(userId);
            var mockModelFiles = CreateMockModelFiles(userId);

            SetupMockDbSets(mockModels, mockCollections, mockLikes, mockComments, mockModelFiles);

            // Act
            var result = await _service.GetUserStatisticsAsync(userId, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(3, result.TotalModels);
            Assert.Equal(2, result.TotalCollections);
            Assert.Equal(5, result.TotalLikes);
            Assert.Equal(15, result.TotalDownloads);
            Assert.Equal(4, result.TotalComments);
            Assert.Equal(1024 * 1024, result.TotalStorageUsedBytes); // 1MB
        }

        [Fact]
        public async Task GetUserStatisticsAsync_UserWithNoModels_ReturnsZeroStatistics()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var emptyModels = new List<Model>();
            var emptyCollections = new List<Collection>();
            var emptyLikes = new List<Like>();
            var emptyComments = new List<CommentDomain.Comment>();
            var emptyModelFiles = new List<ModelFile>();

            SetupMockDbSets(emptyModels, emptyCollections, emptyLikes, emptyComments, emptyModelFiles);

            // Act
            var result = await _service.GetUserStatisticsAsync(userId, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(0, result.TotalModels);
            Assert.Equal(0, result.TotalCollections);
            Assert.Equal(0, result.TotalLikes);
            Assert.Equal(0, result.TotalDownloads);
            Assert.Equal(0, result.TotalComments);
            Assert.Equal(0, result.TotalStorageUsedBytes);
        }

        [Fact]
        public async Task GetUserStatisticsAsync_UserWithDeletedModels_ExcludesDeletedModels()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var mockModels = CreateMockModelsWithDeleted(userId);
            var emptyCollections = new List<Collection>();
            var emptyLikes = new List<Like>();
            var emptyComments = new List<CommentDomain.Comment>();
            var emptyModelFiles = new List<ModelFile>();

            SetupMockDbSets(mockModels, emptyCollections, emptyLikes, emptyComments, emptyModelFiles);

            // Act
            var result = await _service.GetUserStatisticsAsync(userId, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.TotalModels); // Only 1 active model
            Assert.Equal(0, result.TotalDownloads); // Only active model downloads
        }

        private List<Model> CreateMockModels(Guid userId)
        {
            return new List<Model>
            {
                new Model
                {
                    Id = Guid.NewGuid(),
                    AuthorId = userId,
                    Downloads = 5,
                    Likes = 2,
                    DeletedAt = null
                },
                new Model
                {
                    Id = Guid.NewGuid(),
                    AuthorId = userId,
                    Downloads = 7,
                    Likes = 3,
                    DeletedAt = null
                },
                new Model
                {
                    Id = Guid.NewGuid(),
                    AuthorId = userId,
                    Downloads = 3,
                    Likes = 0,
                    DeletedAt = null
                }
            };
        }

        private List<Model> CreateMockModelsWithDeleted(Guid userId)
        {
            return new List<Model>
            {
                new Model
                {
                    Id = Guid.NewGuid(),
                    AuthorId = userId,
                    Downloads = 5,
                    Likes = 2,
                    DeletedAt = null
                },
                new Model
                {
                    Id = Guid.NewGuid(),
                    AuthorId = userId,
                    Downloads = 10,
                    Likes = 5,
                    DeletedAt = DateTime.UtcNow // Deleted model
                }
            };
        }

        private List<Collection> CreateMockCollections(Guid userId)
        {
            return new List<Collection>
            {
                new Collection
                {
                    Id = Guid.NewGuid(),
                    OwnerId = userId,
                    DeletedAt = null
                },
                new Collection
                {
                    Id = Guid.NewGuid(),
                    OwnerId = userId,
                    DeletedAt = null
                }
            };
        }

        private List<Like> CreateMockLikes(Guid userId)
        {
            return new List<Like>
            {
                new Like { UserId = userId, User = new User { Id = userId }, DeletedAt = null },
                new Like { UserId = userId, User = new User { Id = userId }, DeletedAt = null },
                new Like { UserId = userId, User = new User { Id = userId }, DeletedAt = null },
                new Like { UserId = userId, User = new User { Id = userId }, DeletedAt = null },
                new Like { UserId = userId, User = new User { Id = userId }, DeletedAt = null }
            };
        }

        private List<CommentDomain.Comment> CreateMockComments(Guid userId)
        {
            return new List<CommentDomain.Comment>
            {
                new CommentDomain.Comment { Content = "Test comment 1", Author = new User { Id = userId }, Model = new Model { Id = Guid.NewGuid() }, DeletedAt = null },
                new CommentDomain.Comment { Content = "Test comment 2", Author = new User { Id = userId }, Model = new Model { Id = Guid.NewGuid() }, DeletedAt = null },
                new CommentDomain.Comment { Content = "Test comment 3", Author = new User { Id = userId }, Model = new Model { Id = Guid.NewGuid() }, DeletedAt = null },
                new CommentDomain.Comment { Content = "Test comment 4", Author = new User { Id = userId }, Model = new Model { Id = Guid.NewGuid() }, DeletedAt = null }
            };
        }

        private List<ModelFile> CreateMockModelFiles(Guid userId)
        {
            return new List<ModelFile>
            {
                new ModelFile
                {
                    Model = new Model { AuthorId = userId, DeletedAt = null },
                    Size = 512 * 1024, // 512KB
                    DeletedAt = null
                },
                new ModelFile
                {
                    Model = new Model { AuthorId = userId, DeletedAt = null },
                    Size = 512 * 1024, // 512KB
                    DeletedAt = null
                }
            };
        }

        private void SetupMockDbSets(
            List<Model> models,
            List<Collection> collections,
            List<Like> likes,
            List<CommentDomain.Comment> comments,
            List<ModelFile> modelFiles)
        {
            var mockModelsDbSet = CreateMockDbSet(models);
            var mockCollectionsDbSet = CreateMockDbSet(collections);
            var mockLikesDbSet = CreateMockDbSet(likes);
            var mockCommentsDbSet = CreateMockDbSet(comments);
            var mockModelFilesDbSet = CreateMockDbSet(modelFiles);

            _mockDbContext.Setup(c => c.Models).Returns(mockModelsDbSet.Object);
            _mockDbContext.Setup(c => c.Collections).Returns(mockCollectionsDbSet.Object);
            _mockDbContext.Setup(c => c.Likes).Returns(mockLikesDbSet.Object);
            _mockDbContext.Setup(c => c.Comments).Returns(mockCommentsDbSet.Object);
            _mockDbContext.Setup(c => c.ModelFiles).Returns(mockModelFilesDbSet.Object);
        }

        private Mock<DbSet<T>> CreateMockDbSet<T>(List<T> list) where T : class
        {
            var queryable = list.AsQueryable();
            var mockDbSet = new Mock<DbSet<T>>();
            mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            return mockDbSet;
        }
    }
}
