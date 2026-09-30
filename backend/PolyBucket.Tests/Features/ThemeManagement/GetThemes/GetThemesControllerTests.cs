using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.ThemeManagement.Domain;
using PolyBucket.Api.Features.ThemeManagement.GetThemes;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.ThemeManagement.GetThemes;

public class GetThemesControllerTests
{
    private readonly Mock<IMediator> _mediator = new();

    [Fact(DisplayName = "Get themes returns Ok with the theme list response.")]
    public async Task GetThemes_ReturnsOk()
    {
        // Arrange
        var response = new ThemeListResponse();
        _mediator.SetupSend<GetThemesQuery, ThemeListResponse>(response);
        var controller = new GetThemesController(_mediator.Object);

        // Act
        var result = await controller.GetThemes();

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(response);
    }
}
