using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.ThemeManagement.GetThemes.Domain;
using PolyBucket.Api.Features.ThemeManagement.GetThemes.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.ThemeManagement.GetThemes;

public class GetThemesControllerTests
{
    private readonly Mock<IGetThemesService> _service = new();
    private readonly GetThemesController _controller = new(new Mock<IGetThemesService>().Object);

    [Fact(DisplayName = "When the controller is constructed, it is not null.")]
    public void Controller_IsCreated()
    {
        new GetThemesController(_service.Object).ShouldNotBeNull();
    }
}
