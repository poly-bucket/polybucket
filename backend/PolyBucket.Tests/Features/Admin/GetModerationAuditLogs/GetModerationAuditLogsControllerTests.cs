using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Admin.GetModerationAuditLogs.Http;
using PolyBucket.Api.Features.ModelModeration.Domain;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Admin.GetModerationAuditLogs;

public class GetModerationAuditLogsControllerTests : IDisposable
{
    private readonly PolyBucketDbContext _context;
    private readonly Guid _adminUserId;
    private readonly Guid _adminRoleId;

    public GetModerationAuditLogsControllerTests()
    {
        _context = new PolyBucketDbContext(new DbContextOptionsBuilder<PolyBucketDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        _adminRoleId = Guid.NewGuid();
        _adminUserId = Guid.NewGuid();
        _context.Roles.Add(new Role { Id = _adminRoleId, Name = "Admin", Description = "Admin", IsActive = true });
        _context.Users.Add(new User
        {
            Id = _adminUserId,
            Email = "admin@test.com",
            Username = "admin",
            Salt = "s",
            PasswordHash = "h",
            RoleId = _adminRoleId,
            Role = _context.Roles.Find(_adminRoleId)
        });
        _context.SaveChanges();
    }

    private GetModerationAuditLogsController CreateController(Guid? userId) =>
        new GetModerationAuditLogsController(_context).WithUser(userId);

    [Fact(DisplayName = "When the caller is an admin, GetAuditLogs returns Ok with paginated logs.")]
    public async Task GetAuditLogs_Admin_ReturnsOk()
    {
        // Arrange
        _context.ModerationAuditLogs.Add(new ModerationAuditLog
        {
            Id = Guid.NewGuid(),
            ModelId = Guid.NewGuid(),
            Action = ModerationAction.Approve,
            PerformedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await CreateController(_adminUserId).GetAuditLogs();

        // Assert
        var ok = result.Result.ShouldBeOfType<OkObjectResult>();
        var body = ok.Value.ShouldBeOfType<ModerationAuditResponse>();
        body.TotalCount.ShouldBe(1);
        body.Logs.Count.ShouldBe(1);
    }

    [Fact(DisplayName = "When the caller has no user id, GetAuditLogs returns Unauthorized.")]
    public async Task GetAuditLogs_NoUser_ReturnsUnauthorized()
    {
        // Act
        var result = await CreateController(null).GetAuditLogs();

        // Assert
        result.Result.ShouldBeOfType<UnauthorizedObjectResult>();
    }

    [Fact(DisplayName = "When the caller is not an admin, GetAuditLogs returns Forbid.")]
    public async Task GetAuditLogs_NonAdmin_ReturnsForbid()
    {
        // Arrange
        var userRoleId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _context.Roles.Add(new Role { Id = userRoleId, Name = "User", Description = "User", IsActive = true });
        _context.Users.Add(new User
        {
            Id = userId,
            Email = "user@test.com",
            Username = "user",
            Salt = "s",
            PasswordHash = "h",
            RoleId = userRoleId,
            Role = _context.Roles.Find(userRoleId)
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await CreateController(userId).GetAuditLogs();

        // Assert
        result.Result.ShouldBeOfType<ForbidResult>();
    }

    public void Dispose() => _context.Dispose();
}
