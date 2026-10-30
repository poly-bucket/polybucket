using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.ThemeManagement.GetActiveTheme.Domain;
using PolyBucket.Api.Features.ThemeManagement.GetActiveTheme.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.ThemeManagement.GetActiveTheme;

public class GetActiveThemeControllerTests
{
    private readonly Mock<IGetActiveThemeService> _service = new();
    private readonly GetActiveThemeController _controller = new(new Mock<IGetActiveThemeService>().Object);

    [Fact(DisplayName = "When the controller is constructed, it is not null.")]
    public void Controller_IsCreated()
    {
        new GetActiveThemeController(_service.Object).ShouldNotBeNull();
    }
}
