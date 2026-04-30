using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Models.GetModelByUserId.Domain;
using PolyBucket.Api.Features.Models.GetModelByUserId.Http;
using PolyBucket.Api.Features.Models.GetModelByUserId.Repository;
using PolyBucket.Api.Common.Storage;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.GetModelByUserId
{
    public class GetModelByUserIdServiceTests
    {
        private readonly Mock<IGetModelByUserIdRepository> _mockRepository;
        private readonly Mock<IPermissionService> _mockPermissionService;
        private readonly Mock<ILogger<GetModelByUserIdService>> _mockLogger;
        private readonly Mock<IStorageService> _mockStorageService;
        private readonly GetModelByUserIdService _service;

        public GetModelByUserIdServiceTests()
        {
            _mockRepository = new Mock<IGetModelByUserIdRepository>();
            _mockPermissionService = new Mock<IPermissionService>();
            _mockLogger = new Mock<ILogger<GetModelByUserIdService>>();
            _mockStorageService = new Mock<IStorageService>();
            _service = new GetModelByUserIdService(_mockRepository.Object, _mockPermissionService.Object, _mockLogger.Object, _mockStorageService.Object);
        }

        [Fact(DisplayName = "When getting models for the current user's own id, the get model by user id service returns their models.")]
        public async Task GetModelsByUserIdAsync_WithOwnModels_ShouldReturnModels()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new GetModelByUserIdRequest
            {
                Page = 1,
                Take = 10,
                IncludePrivate = true,
                IncludeDeleted = false
            };
            var user = CreateTestUser(userId);
            var models = CreateTestModels(userId);
            var cancellationToken = CancellationToken.None;

            _mockPermissionService.Setup(x => x.HasPermissionAsync(userId, It.IsAny<string>()))
                .ReturnsAsync(false); // User doesn't have MODEL_VIEW_PRIVATE permission

            _mockRepository.Setup(x => x.GetModelsByUserIdAsync(userId, request.Page, request.Take, true, false, cancellationToken))
                .ReturnsAsync((models, 1));

            // Act
            var result = await _service.GetModelsByUserIdAsync(userId, request, user, cancellationToken);

            // Assert
            result.ShouldNotBeNull();
            result.Models.ShouldNotBeNull();
            result.Models.ShouldHaveSingleItem();
            result.TotalCount.ShouldBe(1);
            result.Page.ShouldBe(1);
            result.Take.ShouldBe(10);
        }

        [Fact(DisplayName = "When getting another user's models without the required permission, the get model by user id service throws a ValidationException.")]
        public async Task GetModelsByUserIdAsync_WithOtherUserModels_ShouldThrowValidationException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var request = new GetModelByUserIdRequest { Page = 1, Take = 10 };
            var user = CreateTestUser(userId);
            var cancellationToken = CancellationToken.None;

            _mockPermissionService.Setup(x => x.HasPermissionAsync(userId, It.IsAny<string>()))
                .ReturnsAsync(false); // User doesn't have MODEL_VIEW_PRIVATE permission

            // Act & Assert
            await Should.ThrowAsync<ValidationException>(async () =>
                await _service.GetModelsByUserIdAsync(otherUserId, request, user, cancellationToken));
        }

        [Fact(DisplayName = "When getting another user's models with the MODEL_VIEW_PRIVATE permission, the get model by user id service allows access.")]
        public async Task GetModelsByUserIdAsync_WithAdminPermission_ShouldAllowAccessToOtherUserModels()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var request = new GetModelByUserIdRequest { Page = 1, Take = 10 };
            var user = CreateTestUser(userId);
            var models = CreateTestModels(otherUserId);
            var cancellationToken = CancellationToken.None;

            _mockPermissionService.Setup(x => x.HasPermissionAsync(userId, It.IsAny<string>()))
                .ReturnsAsync(true); // User has MODEL_VIEW_PRIVATE permission

            _mockRepository.Setup(x => x.GetModelsByUserIdAsync(otherUserId, request.Page, request.Take, false, false, cancellationToken))
                .ReturnsAsync((models, 1));

            // Act
            var result = await _service.GetModelsByUserIdAsync(otherUserId, request, user, cancellationToken);

            // Assert
            result.ShouldNotBeNull();
            result.Models.ShouldNotBeNull();
            result.Models.ShouldHaveSingleItem();
        }

        [Fact(DisplayName = "When getting models with an invalid user principal, the get model by user id service throws a ValidationException.")]
        public async Task GetModelsByUserIdAsync_WithInvalidUser_ShouldThrowValidationException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new GetModelByUserIdRequest { Page = 1, Take = 10 };
            var user = CreateInvalidUser();
            var cancellationToken = CancellationToken.None;

            // Act & Assert
            await Should.ThrowAsync<ValidationException>(async () =>
                await _service.GetModelsByUserIdAsync(userId, request, user, cancellationToken));
        }

        [Fact(DisplayName = "When getting models with pagination parameters, the get model by user id service calculates the total page count correctly.")]
        public async Task GetModelsByUserIdAsync_WithPagination_ShouldCalculateTotalPages()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new GetModelByUserIdRequest
            {
                Page = 2,
                Take = 5
            };
            var user = CreateTestUser(userId);
            var models = CreateTestModels(userId, 12); // 12 total models
            var cancellationToken = CancellationToken.None;

            _mockPermissionService.Setup(x => x.HasPermissionAsync(userId, It.IsAny<string>()))
                .ReturnsAsync(false);

            _mockRepository.Setup(x => x.GetModelsByUserIdAsync(userId, request.Page, request.Take, false, false, cancellationToken))
                .ReturnsAsync((models, 12)); // 12 total count

            // Act
            var result = await _service.GetModelsByUserIdAsync(userId, request, user, cancellationToken);

            // Assert
            result.ShouldNotBeNull();
            result.TotalCount.ShouldBe(12);
            result.TotalPages.ShouldBe(3); // 12 models / 5 per page = 3 pages
        }

        private static ClaimsPrincipal CreateTestUser(Guid userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, "test@example.com")
            };

            return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        }

        private static ClaimsPrincipal CreateInvalidUser()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, "test@example.com")
                // Missing NameIdentifier claim
            };

            return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        }

        private static IEnumerable<Model> CreateTestModels(Guid authorId, int count = 1)
        {
            var models = new List<Model>();
            for (int i = 0; i < count; i++)
            {
                models.Add(new Model
                {
                    Id = Guid.NewGuid(),
                    Name = $"Test Model {i + 1}",
                    Description = $"Test Description {i + 1}",
                    AuthorId = authorId
                });
            }
            return models;
        }
    }
} 