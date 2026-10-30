using System.Net;
using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using PolyBucket.Api.Features.Plugins.Http;
using PolyBucket.Api.Features.Plugins.Services;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Plugins.Http;

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
        var marketplaceClient = new MarketplaceClient(
            httpClient,
            new FixedMarketplaceBaseUrlResolver("https://marketplace.test"),
            NullLogger<MarketplaceClient>.Instance);
        var installationService = new PluginInstallationService(
            new EmptyArchiveDownloader(),
            new PluginManifestValidator(NullLogger<PluginManifestValidator>.Instance),
            new TestHostApplicationLifetime(),
            NullLogger<PluginInstallationService>.Instance);
        return new MarketplacePluginController(
            marketplaceClient,
            installationService,
            NullLogger<MarketplacePluginController>.Instance);
    }

    private sealed class FixedMarketplaceBaseUrlResolver(string baseUrl) : IMarketplaceBaseUrlResolver
    {
        public Task<string> ResolveAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(baseUrl);
    }

    private sealed class EmptyArchiveDownloader : IPluginArchiveDownloader
    {
        public Task<PluginArchiveDownloadResult> DownloadRepositoryArchiveAsync(
            string repositoryUrl,
            string branch,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new PluginArchiveDownloadResult { Success = false, Message = "not used" });
    }

    private sealed class TestHostApplicationLifetime : IHostApplicationLifetime
    {
        public CancellationToken ApplicationStarted { get; } = CancellationToken.None;
        public CancellationToken ApplicationStopping { get; } = CancellationToken.None;
        public CancellationToken ApplicationStopped { get; } = CancellationToken.None;
        public void StopApplication()
        {
        }
    }

    private sealed class DelegatingHandlerStub : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

        public DelegatingHandlerStub(Func<HttpRequestMessage, HttpResponseMessage> responder) => _responder = responder;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(_responder(request));
    }
}
