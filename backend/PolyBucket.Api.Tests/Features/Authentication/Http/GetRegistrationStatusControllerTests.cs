using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.Authentication.GetRegistrationStatus.Domain;
using PolyBucket.Api.Features.Authentication.GetRegistrationStatus.Http;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Database.Entities.SystemSettings;
using PolyBucket.Database.Persistence;
using Shouldly;
using System;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Authentication.Http;

public class GetRegistrationStatusControllerTests : IDisposable
{
    private readonly PolyBucketDbContext _dbContext;
    private readonly GetRegistrationStatusController _controller;

    public GetRegistrationStatusControllerTests()
    {
        var dbOptions = new DbContextOptionsBuilder<PolyBucketDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new PolyBucketDbContext(dbOptions);
        _controller = new GetRegistrationStatusController(new GetRegistrationStatusService(new UserRegistrationGate(_dbContext)));
    }

    [Fact(DisplayName = "When no site setup exists, registration status reports registration is closed.")]
    public async Task GetRegistrationStatus_NoSetup_ReturnsFalse()
    {
        // Act
        var result = await _controller.GetRegistrationStatus(CancellationToken.None);

        // Assert
        var ok = result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldNotBeNull();
        var allowed = ok.Value!.GetType().GetProperty("allowUserRegistration")!.GetValue(ok.Value);
        allowed.ShouldBe(false);
    }

    [Fact(DisplayName = "When registration is enabled in site setup, registration status reports open.")]
    public async Task GetRegistrationStatus_Open_ReturnsTrue()
    {
        // Arrange
        _dbContext.SystemSetups.Add(new SystemSetup
        {
            Id = Guid.NewGuid(),
            AllowUserRegistration = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _controller.GetRegistrationStatus(CancellationToken.None);

        // Assert
        var ok = result.ShouldBeOfType<OkObjectResult>();
        var allowed = ok.Value!.GetType().GetProperty("allowUserRegistration")!.GetValue(ok.Value);
        allowed.ShouldBe(true);
    }

    [Fact(DisplayName = "When registration is disabled in site setup, registration status reports closed.")]
    public async Task GetRegistrationStatus_Closed_ReturnsFalse()
    {
        // Arrange
        _dbContext.SystemSetups.Add(new SystemSetup
        {
            Id = Guid.NewGuid(),
            AllowUserRegistration = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _controller.GetRegistrationStatus(CancellationToken.None);

        // Assert
        var ok = result.ShouldBeOfType<OkObjectResult>();
        var allowed = ok.Value!.GetType().GetProperty("allowUserRegistration")!.GetValue(ok.Value);
        allowed.ShouldBe(false);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
