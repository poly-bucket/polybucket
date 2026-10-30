using Microsoft.AspNetCore.Mvc;
using Moq;
using PolyBucket.Api.Features.ThemeManagement.SetActiveTheme.Domain;
using PolyBucket.Api.Features.ThemeManagement.SetActiveTheme.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.ThemeManagement.SetActiveTheme;

public class SetActiveThemeControllerTests
{
    private readonly Mock<ISetActiveThemeService> _service = new();
    private readonly SetActiveThemeController _controller = new(new Mock<ISetActiveThemeService>().Object);

    [Fact(DisplayName = "When the controller is constructed, it is not null.")]
    public void Controller_IsCreated()
    {
        new SetActiveThemeController(_service.Object).ShouldNotBeNull();
    }
}
