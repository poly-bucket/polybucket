using System;
using System.Text.Json;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Entities.Models.Enums;
using PolyBucket.Api.Features.ModelModeration.ModeratorEditModel.Domain;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.ModelModeration.ModeratorEditModel;

public class ModeratorEditModelSanitizationTests
{
    [Fact(DisplayName = "When mapping a model for moderation, the response DTO does not leak author credentials.")]
    public void ToResponseDto_DoesNotLeakSensitiveAuthorData()
    {
        // Arrange
        var model = new Model
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Description = "Desc",
            Privacy = PrivacySettings.Public,
            AuthorId = Guid.NewGuid(),
            Author = new User
            {
                Username = "alice",
                Email = "alice@example.com",
                Salt = "super-secret-salt",
                PasswordHash = "super-secret-hash"
            }
        };

        // Act
        var dto = ModeratorEditModelMapper.ToResponseDto(model);
        var json = JsonSerializer.Serialize(dto);

        // Assert
        dto.Author!.Username.ShouldBe("alice");
        json.ShouldNotContain("super-secret-hash");
        json.ShouldNotContain("super-secret-salt");
        json.ShouldNotContain("passwordHash", Case.Insensitive);
        json.ShouldNotContain("alice@example.com");
    }
}
