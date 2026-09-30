using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.Http;
using PolyBucket.Api.Features.Plugins.Services;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Plugins.Http;

public class MarketplacePluginControllerTests
{
    [Fact(DisplayName = "When marketplace plugins load, GetMarketplacePlugins returns Ok.")]
    public async Task GetMarketplacePlugins_Success_ReturnsOk()
    {
        // Arrange
        var payload = new MarketplacePluginsResponse { TotalCount = 0, Page = 1, PageSize = 20 };
        var controller = CreateController(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload))
        });

        // Act
        var result = await controller.GetMarketplacePlugins();

        // Assert
        result.Result.ShouldBeOfType<OkObjectResult>().Value.ShouldBeOfType<MarketplacePluginsResponse>();
    }

    [Fact(DisplayName = "When plugin id is empty, InstallFromMarketplace returns BadRequest.")]
    public async Task InstallFromMarketplace_EmptyPluginId_ReturnsBadRequest()
    {
        // Arrange
        var controller = CreateController(_ => new HttpResponseMessage(HttpStatusCode.OK));

        // Act
        var result = await controller.InstallFromMarketplace("  ");

        // Assert
        result.Result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "When plugin details are missing, GetMarketplacePlugin returns NotFound.")]
    public async Task GetMarketplacePlugin_Missing_ReturnsNotFound()
    {
        // Arrange
        var controller = CreateController(_ => new HttpResponseMessage(HttpStatusCode.NotFound));

        // Act
        var result = await controller.GetMarketplacePlugin("missing-plugin");

        // Assert
        result.Result.ShouldBeOfType<NotFoundObjectResult>();
    }

    private static MarketplacePluginController CreateController(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        var handler = new DelegatingHandlerStub(responder);
        var httpClient = new HttpClient(handler);
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Marketplace:BaseUrl"] = "https://marketplace.test" })
            .Build();
        var marketplaceClient = new MarketplaceClient(httpClient, configuration, NullLogger<MarketplaceClient>.Instance);
        var installationService = new PluginInstallationService(
            new HttpClient(new DelegatingHandlerStub(_ => new HttpResponseMessage(HttpStatusCode.OK))),
            new PluginManifestValidator(NullLogger<PluginManifestValidator>.Instance),
            NullLogger<PluginInstallationService>.Instance);
        return new MarketplacePluginController(
            marketplaceClient,
            installationService,
            NullLogger<MarketplacePluginController>.Instance);
    }

    private sealed class DelegatingHandlerStub : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

        public DelegatingHandlerStub(Func<HttpRequestMessage, HttpResponseMessage> responder) => _responder = responder;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(_responder(request));
    }
}
