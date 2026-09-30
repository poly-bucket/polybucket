using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.Protected;
using PolyBucket.Api.Features.Federation.Domain;
using PolyBucket.Api.Features.Federation.Http;
using PolyBucket.Api.Features.Federation.Repository;
using PolyBucket.Tests.Testing;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Federation.GetFederationHealth;

public class GetFederationHealthControllerTests
{
    private readonly Mock<IFederationRepository> _repository = new();

    [Fact(DisplayName = "When the instance is missing, GetFederationHealth returns NotFound.")]
    public async Task GetFederationHealth_Missing_ReturnsNotFound()
    {
        // Arrange
        _repository.Setup(r => r.GetFederatedInstanceAsync(It.IsAny<Guid>())).ReturnsAsync((FederatedInstance?)null);
        var factory = new Mock<IHttpClientFactory>();
        var controller = new GetFederationHealthController(_repository.Object, factory.Object).WithUser(Guid.NewGuid());

        // Act
        var result = await controller.GetFederationHealth(Guid.NewGuid());

        // Assert
        result.Result.ShouldBeOfType<NotFoundObjectResult>();
    }

    [Fact(DisplayName = "When the instance exists, GetFederationHealth returns Ok with health metadata.")]
    public async Task GetFederationHealth_Found_ReturnsOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repository.Setup(r => r.GetFederatedInstanceAsync(id)).ReturnsAsync(new FederatedInstance
        {
            Id = id,
            Name = "Peer",
            BaseUrl = "https://peer.example",
            Status = FederationStatus.Active,
            CreatedAt = DateTime.UtcNow
        });
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
        var client = new HttpClient(handler.Object);
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);
        var controller = new GetFederationHealthController(_repository.Object, factory.Object).WithUser(Guid.NewGuid());

        // Act
        var result = await controller.GetFederationHealth(id);

        // Assert
        var ok = result.Result.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBeOfType<FederationHealthResponse>().IsReachable.ShouldBeTrue();
    }
}
