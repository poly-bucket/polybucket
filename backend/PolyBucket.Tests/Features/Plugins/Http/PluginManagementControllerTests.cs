using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.Http;
using PolyBucket.Api.Features.Plugins.Services;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Plugins.Http;

public class PluginManagementControllerTests
{
    [Fact(DisplayName = "When installed plugins are listed, GetInstalledPlugins returns Ok.")]
    public async Task GetInstalledPlugins_ReturnsOk()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.GetInstalledPlugins();

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBeAssignableTo<System.Collections.Generic.List<InstalledPlugin>>();
    }

    [Fact(DisplayName = "When installation source is missing, InstallPlugin returns BadRequest.")]
    public async Task InstallPlugin_MissingSource_ReturnsBadRequest()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.InstallPlugin(new PluginInstallationRequest { Source = " " });

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "When plugin id is empty, UninstallPlugin returns BadRequest.")]
    public async Task UninstallPlugin_EmptyId_ReturnsBadRequest()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.UninstallPlugin(" ");

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }

    private static PluginManagementController CreateController()
    {
        var installationService = new PluginInstallationService(
            new HttpClient(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK))),
            new PluginManifestValidator(NullLogger<PluginManifestValidator>.Instance),
            NullLogger<PluginInstallationService>.Instance);
        return new PluginManagementController(installationService, NullLogger<PluginManagementController>.Instance);
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly System.Func<HttpRequestMessage, HttpResponseMessage> _respond;

        public StubHandler(System.Func<HttpRequestMessage, HttpResponseMessage> respond) => _respond = respond;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(_respond(request));
    }
}
