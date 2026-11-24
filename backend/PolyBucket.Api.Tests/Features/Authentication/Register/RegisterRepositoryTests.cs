using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using PolyBucket.Api.Features.Authentication.Register.Repository;
using PolyBucket.Database.Entities.ACL;
using PolyBucket.Database.Entities.SystemSettings;
using PolyBucket.Database.Persistence;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Authentication.Register;

public class RegisterRepositoryTests : IDisposable
{
    private readonly PolyBucketDbContext _context;
    private readonly RegisterRepository _repository;

    public RegisterRepositoryTests()
    {
        _context = new PolyBucketDbContext(new DbContextOptionsBuilder<PolyBucketDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        _repository = new RegisterRepository(_context, NullLogger<RegisterRepository>.Instance);
    }

    [Fact(DisplayName = "GetDefaultUserRoleIdAsync returns the role configured on system setup.")]
    public async Task GetDefaultUserRoleIdAsync_UsesSystemSetupRole()
    {
        // Arrange
        var moderatorId = Guid.NewGuid();
        _context.Roles.AddRange(
            new Role { Id = Guid.NewGuid(), Name = "User", IsActive = true },
            new Role { Id = moderatorId, Name = "Moderator", IsActive = true });
        _context.SystemSetups.Add(new SystemSetup
        {
            Id = Guid.NewGuid(),
            DefaultUserRole = "Moderator",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        var roleId = await _repository.GetDefaultUserRoleIdAsync(CancellationToken.None);

        // Assert
        roleId.ShouldBe(moderatorId);
    }

    [Fact(DisplayName = "GetDefaultUserRoleIdAsync falls back to User when the configured role is missing.")]
    public async Task GetDefaultUserRoleIdAsync_FallsBackToUser()
    {
        // Arrange
        var userRoleId = Guid.NewGuid();
        _context.Roles.Add(new Role { Id = userRoleId, Name = "User", IsActive = true });
        _context.SystemSetups.Add(new SystemSetup
        {
            Id = Guid.NewGuid(),
            DefaultUserRole = "CustomMissing",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        var roleId = await _repository.GetDefaultUserRoleIdAsync(CancellationToken.None);

        // Assert
        roleId.ShouldBe(userRoleId);
    }

    public void Dispose() => _context.Dispose();
}