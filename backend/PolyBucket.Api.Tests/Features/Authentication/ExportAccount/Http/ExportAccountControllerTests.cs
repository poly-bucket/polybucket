using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Persistence;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Authentication.ExportAccount.Domain;
using PolyBucket.Api.Features.Authentication.ExportAccount.Http;
using PolyBucket.Api.Features.Authentication.ExportAccount.Repository;
using DomainUserSettings = PolyBucket.Database.Entities.Users.UserSettings;
using PolyBucket.Api.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Authentication.ExportAccount.Http;

public class ExportAccountControllerTests : IDisposable
{
    private readonly PolyBucketDbContext _context;
    private readonly Guid _userId;

    public ExportAccountControllerTests()
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
            Email = "export@test.com",
            Username = "exportuser",
            Salt = "s",
            PasswordHash = "h",
            RoleId = roleId,
            Role = _context.Roles.Find(roleId),
            Settings = new DomainUserSettings
            {
                UserId = _userId,
                Language = "en",
                Theme = "light",
                EmailNotifications = true,
                MeasurementSystem = "metric",
                TimeZone = "UTC"
            }
        });
        _context.SaveChanges();
    }

    [Fact(DisplayName = "When the signed-in user exists, Export returns Ok with profile data.")]
    public async Task Export_ValidUser_ReturnsOk()
    {
        // Arrange
        var service = new ExportAccountService(new ExportAccountRepository(_context));
        var controller = new ExportAccountController(service).WithUser(_userId);

        // Act
        var result = await controller.Export(CancellationToken.None);

        // Assert
        var ok = result.Result.ShouldBeOfType<OkObjectResult>();
        var export = ok.Value.ShouldBeOfType<AccountExportResponse>();
        export.UserId.ShouldBe(_userId);
        export.Email.ShouldBe("export@test.com");
        export.Username.ShouldBe("exportuser");
        export.Settings.ShouldNotBeNull();
        export.Settings!.Language.ShouldBe("en");
    }

    [Fact(DisplayName = "When the token has no user id, Export returns Unauthorized.")]
    public async Task Export_NoUserClaim_ReturnsUnauthorized()
    {
        // Arrange
        var service = new ExportAccountService(new ExportAccountRepository(_context));

        // Act
        var result = await new ExportAccountController(service).WithUser(null).Export(CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<UnauthorizedObjectResult>();
    }

    [Fact(DisplayName = "When the user was deleted, Export returns NotFound.")]
    public async Task Export_MissingUser_ReturnsNotFound()
    {
        // Arrange
        var service = new ExportAccountService(new ExportAccountRepository(_context));

        // Act
        var result = await new ExportAccountController(service).WithUser(Guid.NewGuid()).Export(CancellationToken.None);

        // Assert
        result.Result.ShouldBeOfType<NotFoundObjectResult>();
    }

    public void Dispose() => _context.Dispose();
}
