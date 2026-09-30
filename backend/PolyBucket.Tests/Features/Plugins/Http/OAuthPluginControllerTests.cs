using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.Http;
using PolyBucket.Api.Features.Plugins.Services;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Plugins.Http;

public class OAuthPluginControllerTests
{
    [Fact(DisplayName = "When no providers are registered, GetOAuthProviders returns Ok with an empty list.")]
    public void GetOAuthProviders_ReturnsEmptyList()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.GetOAuthProviders();

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBeAssignableTo<System.Collections.Generic.List<OAuthProviderInfo>>();
    }

    [Fact(DisplayName = "When redirect URI is missing, GetAuthorizationUrl returns BadRequest.")]
    public void GetAuthorizationUrl_MissingRedirect_ReturnsBadRequest()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.GetAuthorizationUrl("discord", redirectUri: " ");

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "When the provider is unknown, GetAuthorizationUrl returns NotFound.")]
    public void GetAuthorizationUrl_UnknownProvider_ReturnsNotFound()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.GetAuthorizationUrl("unknown", redirectUri: "https://app/callback");

        // Assert
        result.ShouldBeOfType<NotFoundObjectResult>();
    }

    [Fact(DisplayName = "When authorization succeeds, HandleCallback returns Ok.")]
    public async Task HandleCallback_Success_ReturnsOk()
    {
        // Arrange
        var oauthPlugin = new Mock<IOAuthPlugin>();
        oauthPlugin.SetupGet(p => p.Id).Returns("discord-plugin");
        oauthPlugin.SetupGet(p => p.ProviderName).Returns("discord");
        oauthPlugin
            .Setup(p => p.AuthorizeAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new OAuthAuthorizationResult { Success = true });
        var service = new OAuthPluginService(NullLogger<OAuthPluginService>.Instance);
        await service.RegisterOAuthProviderAsync(oauthPlugin.Object);
        var controller = new OAuthPluginController(service, NullLogger<OAuthPluginController>.Instance);

        // Act
        var result = await controller.HandleCallback(
            "discord",
            new OAuthCallbackRequest { Code = "code", RedirectUri = "https://app/callback" });

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBeOfType<OAuthAuthorizationResult>().Success.ShouldBeTrue();
    }

    private static OAuthPluginController CreateController() =>
        new(new OAuthPluginService(NullLogger<OAuthPluginService>.Instance), NullLogger<OAuthPluginController>.Instance);
}
