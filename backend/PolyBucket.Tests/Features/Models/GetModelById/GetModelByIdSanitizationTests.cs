using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Common.Models.Enums;
using PolyBucket.Api.Common.Storage;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.Models.GetModelById.Domain;
using PolyBucket.Api.Features.Models.GetModelById.Repository;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.GetModelById
{
    public class GetModelByIdSanitizationTests
    {
        private readonly Mock<IGetModelByIdRepository> _mockRepository;
        private readonly Mock<ILogger<GetModelByIdQueryHandler>> _mockLogger;
        private readonly Mock<IPermissionService> _mockPermissionService;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<IStorageService> _mockStorage;
        private readonly GetModelByIdQueryHandler _handler;

        public GetModelByIdSanitizationTests()
        {
            _mockRepository = new Mock<IGetModelByIdRepository>();
            _mockLogger = new Mock<ILogger<GetModelByIdQueryHandler>>();
            _mockPermissionService = new Mock<IPermissionService>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockStorage = new Mock<IStorageService>();
            _handler = new GetModelByIdQueryHandler(
                _mockRepository.Object,
                _mockLogger.Object,
                _mockPermissionService.Object,
                _mockHttpContextAccessor.Object,
                _mockStorage.Object);
        }

        [Fact(DisplayName = "When getting a public model, the handler returns the author without leaking credentials or other sensitive account data.")]
        public async Task Handle_PublicModel_DoesNotLeakSensitiveAuthorData()
        {
            // Arrange
            var modelId = Guid.NewGuid();
            var model = new Model
            {
                Id = modelId,
                Name = "Test",
                Privacy = PrivacySettings.Public,
                AuthorId = Guid.NewGuid(),
                ThumbnailUrl = "models/test/preview.png",
                Author = new User
                {
                    Username = "alice",
                    Email = "alice@example.com",
                    Salt = "super-secret-salt",
                    PasswordHash = "super-secret-hash"
                }
            };

            _mockRepository.Setup(x => x.GetModelByIdAsync(modelId)).ReturnsAsync(model);
            _mockStorage
                .Setup(x => x.GetPresignedUrlAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("https://storage.example.com/presigned");

            // Act
            var response = await _handler.Handle(new GetModelByIdQuery { Id = modelId }, CancellationToken.None);

            // Assert
            response.Model.Author.ShouldNotBeNull();
            response.Model.Author!.Username.ShouldBe("alice");

            var json = JsonSerializer.Serialize(response);
            json.ShouldNotContain("super-secret-hash");
            json.ShouldNotContain("super-secret-salt");
            json.ShouldNotContain("passwordHash", Case.Insensitive);
            json.ShouldNotContain("salt", Case.Insensitive);
            json.ShouldNotContain("userPermissions", Case.Insensitive);
            json.ShouldNotContain("twoFactorAuth", Case.Insensitive);
        }
    }
}
