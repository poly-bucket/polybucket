using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.Models.Http;
using PolyBucket.Api.Features.Models.RecordModelView.Domain;
using PolyBucket.Api.Features.Models.RecordModelView.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Models.RecordModelView;

public class RecordModelViewControllerTests
{
    private readonly Mock<IRecordModelViewService> _service = new();
    private readonly Mock<IHttpContextAccessor> _httpContextAccessor = new();

    public RecordModelViewControllerTests()
    {
        var context = new DefaultHttpContext();
        _httpContextAccessor.Setup(a => a.HttpContext).Returns(context);
    }

    [Fact(DisplayName = "When a view is recorded, the controller returns Ok with views and counted.")]
    public async Task RecordModelView_ReturnsOkWithBody()
    {
        var modelId = Guid.NewGuid();
        _service.Setup(s => s.RecordViewAsync(
                modelId,
                It.IsAny<System.Security.Claims.ClaimsPrincipal>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(RecordModelViewOutcome.Ok(7, counted: true));

        var controller = CreateController();

        var result = await controller.RecordModelView(modelId, CancellationToken.None);

        var body = result.ShouldBeOfType<OkObjectResult>().Value.ShouldBeOfType<ModelViewResponse>();
        body.Views.ShouldBe(7);
        body.Counted.ShouldBeTrue();
    }

    [Fact(DisplayName = "When the model is missing, the controller returns NotFound.")]
    public async Task RecordModelView_Missing_ReturnsNotFound()
    {
        _service.Setup(s => s.RecordViewAsync(
                It.IsAny<Guid>(),
                It.IsAny<System.Security.Claims.ClaimsPrincipal>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(RecordModelViewOutcome.NotFound());

        var controller = CreateController();

        var result = await controller.RecordModelView(Guid.NewGuid(), CancellationToken.None);

        result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact(DisplayName = "When the viewer cannot access the model, the controller returns Forbid.")]
    public async Task RecordModelView_Forbidden_ReturnsForbid()
    {
        _service.Setup(s => s.RecordViewAsync(
                It.IsAny<Guid>(),
                It.IsAny<System.Security.Claims.ClaimsPrincipal>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(RecordModelViewOutcome.Forbid());

        var controller = CreateController();

        var result = await controller.RecordModelView(Guid.NewGuid(), CancellationToken.None);

        result.ShouldBeOfType<ForbidResult>();
    }

    private RecordModelViewController CreateController() =>
        new(_service.Object, _httpContextAccessor.Object);
}
