using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Authentication.RecoveryCodes.RegenerateRecoveryCodes.Domain;
using PolyBucket.Api.Features.Authentication.RecoveryCodes.RegenerateRecoveryCodes.Http;
using PolyBucket.Api.Features.Authentication.RecoveryCodes.RegenerateRecoveryCodes.Repository;
using PolyBucket.Api.Features.Authentication.Services;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Entities.Users;
using Shouldly;
using System.Security.Claims;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Authentication.RecoveryCodes.RegenerateRecoveryCodes.Http;

public class RegenerateRecoveryCodesControllerTests
{
    [Fact(DisplayName = "When regeneration succeeds, the controller returns Ok with recovery codes.")]
    public async Task Regenerate_ReturnsOkWithCodes()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var repository = new Mock<IRegenerateRecoveryCodesRepository>();
        repository.Setup(r => r.GetUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId, Email = "user@test.com", Username = "user" });

        var provisioning = new Mock<IRecoveryCodeProvisioningService>();
        provisioning.Setup(p => p.ProvisionForNewUserAsync(
                It.IsAny<User>(),
                userId,
                false,
                UserAuditAction.RecoveryCodesRegenerated,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { "CODE-A", "CODE-B" });

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(a => a.HttpContext).Returns(new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) },
                "Test"))
        });

        var handler = new RegenerateRecoveryCodesCommandHandler(
            httpContextAccessor.Object,
            repository.Object,
            provisioning.Object);
        var controller = new RegenerateRecoveryCodesController(handler);

        // Act
        var result = await controller.Regenerate(CancellationToken.None);

        // Assert
        var ok = result.ShouldBeOfType<OkObjectResult>();
        var body = ok.Value.ShouldBeOfType<RegenerateRecoveryCodesResponse>();
        body.RecoveryCodes.Count.ShouldBe(2);
    }
}
