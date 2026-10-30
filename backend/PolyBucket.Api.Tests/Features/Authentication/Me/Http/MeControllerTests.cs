using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Persistence;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Authentication.Me.Domain;
using PolyBucket.Api.Features.Authentication.Me.Http;
using PolyBucket.Api.Features.Authentication.Me.Repository;
using DomainUserSettings = PolyBucket.Database.Entities.Users.UserSettings;
using PolyBucket.Api.Tests.Testing;
using Moq;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Authentication.Me.Http;

public class MeControllerTests : IDisposable
{
    private readonly PolyBucketDbContext _context;
    private readonly Mock<IEmailSettingsResolver> _emailSettings = new();
    private readonly Guid _userId;

    public MeControllerTests()
    {
        _context = new PolyBucketDbContext(new DbContextOptionsBuilder<PolyBucketDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        _userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        _context.Roles.Add(new Role { Id = roleId, Name = "User", Description = "User", IsActive = true });
        _context.Users.Add(new User
        {
            Id = _userId,
            Email = "me@test.com",
            Username = "meuser",
            Salt = "s",
            PasswordHash = "h",
            RoleId = roleId,
            Role = _context.Roles.Find(roleId),
            Settings = new DomainUserSettings
            {
                UserId = _userId,
                Language = "en",
                Theme = "dark",
                EmailNotifications = true,
                MeasurementSystem = "metric",
                TimeZone = "UTC"
            }
        });
        _context.SaveChanges();
        _emailSettings.Setup(s => s.GetEffectiveSettingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EffectiveEmailSettings
            {
                Transport = EmailTransportKind.Log,
                FromAddress = "noreply@example.com",
                PublicBaseUrl = "http://localhost:3000",
                RequireEmailVerification = false
            });
    }

    private MeController CreateController(Guid? userId) =>
        new MeController(new MeService(new MeRepository(_context), _emailSettings.Object)).WithUser(userId);

    [Fact(DisplayName = "When the signed-in user exists, GetCurrentUser returns Ok with profile data.")]
    public async Task GetCurrentUser_Valid_ReturnsOk()
    {
        // Act
        var result = await CreateController(_userId).GetCurrentUser(CancellationToken.None);

        // Assert
        var ok = result.Result.ShouldBeOfType<OkObjectResult>();
        var me = ok.Value.ShouldBeOfType<MeResponse>();
        me.Username.ShouldBe("meuser");
        me.Email.ShouldBe("me@test.com");
        me.Role.ShouldBe("User");
        me.EmailDeliveryAvailable.ShouldBeTrue();
    }

    [Fact(DisplayName = "When the token has no user id, GetCurrentUser returns Unauthorized.")]
    public async Task GetCurrentUser_NoClaim_ReturnsUnauthorized()
    {
        // Act
        var result = await CreateController(null).GetCurrentUser(CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<UnauthorizedObjectResult>();
    }

    [Fact(DisplayName = "When the user was deleted, GetCurrentUser returns NotFound.")]
    public async Task GetCurrentUser_MissingUser_ReturnsNotFound()
    {
        // Act
        var result = await CreateController(Guid.NewGuid()).GetCurrentUser(CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<NotFoundObjectResult>();
    }

    public void Dispose() => _context.Dispose();
}
