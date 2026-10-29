using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Api.Features.Collections.Domain;
using PolyBucket.Api.Features.Collections.Domain.Enums;
using PolyBucket.Api.Features.Collections.GetCollectionById.Domain;
using PolyBucket.Api.Features.Collections.GetCollectionById.Repository;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Collections.GetCollectionById;

public class GetCollectionByIdSanitizationTests
{
    private readonly Mock<ICollectionRepository> _repository = new();
    private readonly Mock<IHttpContextAccessor> _httpContextAccessor = new();
    private readonly GetCollectionByIdQueryHandler _handler;

    public GetCollectionByIdSanitizationTests()
    {
        _httpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);
        _handler = new GetCollectionByIdQueryHandler(_repository.Object, _httpContextAccessor.Object);
    }

    [Fact(DisplayName = "When getting a public collection, the handler returns a DTO without leaking owner credentials or password hashes.")]
    public async Task Handle_PublicCollection_DoesNotLeakSensitiveData()
    {
        // Arrange
        var collectionId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var collection = new Collection
        {
            Id = collectionId,
            Name = "Public Set",
            Visibility = CollectionVisibility.Public,
            OwnerId = ownerId,
            PasswordHash = "hashed-collection-password",
            Owner = new User
            {
                Id = ownerId,
                Username = "alice",
                Email = "alice@example.com",
                Salt = "super-secret-salt",
                PasswordHash = "super-secret-hash"
            }
        };

        _repository.Setup(r => r.GetCollectionByIdAsync(collectionId)).ReturnsAsync(collection);

        // Act
        var response = await _handler.Handle(new GetCollectionByIdQuery { Id = collectionId }, CancellationToken.None);

        // Assert
        response.ShouldNotBeNull();
        response!.IsPasswordProtected.ShouldBeTrue();
        response.Owner!.Username.ShouldBe("alice");

        var json = JsonSerializer.Serialize(response);
        json.ShouldNotContain("super-secret-hash");
        json.ShouldNotContain("super-secret-salt");
        json.ShouldNotContain("hashed-collection-password");
        json.ShouldNotContain("passwordHash", Case.Insensitive);
        json.ShouldNotContain("salt", Case.Insensitive);
        json.ShouldNotContain("alice@example.com");
    }
}
