using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Admin.GetModerationAuditLogs.Domain;
using PolyBucket.Api.Features.Admin.GetModerationAuditLogs.Http;
using PolyBucket.Api.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Admin.GetModerationAuditLogs;

public class GetModerationAuditLogsControllerTests
{
    private readonly Mock<IGetModerationAuditLogsService> _service = new();
    private readonly Guid _adminUserId = Guid.NewGuid();

    private GetModerationAuditLogsController CreateController(Guid? userId) =>
        new GetModerationAuditLogsController(_service.Object).WithUser(userId);

    [Fact(DisplayName = "When the caller is an admin, GetAuditLogs returns Ok with paginated logs.")]
    public async Task GetAuditLogs_Admin_ReturnsOk()
    {
        // Arrange
        var body = new ModerationAuditResponseDto { TotalCount = 1, Logs = { new ModerationAuditLogItemDto { Id = Guid.NewGuid().ToString() } } };
        _service.Setup(s => s.GetAuditLogsAsync(_adminUserId, It.IsAny<GetModerationAuditLogsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(body);

        // Act
        var result = await CreateController(_adminUserId).GetAuditLogs();

        // Assert
        var ok = result.Result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBeOfType<ModerationAuditResponseDto>().TotalCount.ShouldBe(1);
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
        _service.Setup(s => s.GetAuditLogsAsync(It.IsAny<Guid>(), It.IsAny<GetModerationAuditLogsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ModerationAuditResponseDto?)null);

        // Act
        var result = await CreateController(Guid.NewGuid()).GetAuditLogs();

        // Assert
        result.Result.ShouldBeOfType<ForbidResult>();
    }
}
