using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using PolyBucket.Api.Features.Federation.Http;
using PolyBucket.Api.Features.Federation.Services;
using PolyBucket.Api.Features.SystemSettings.Services;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Federation.Http;

public class RequireFederationEnabledFilterTests
{
  private static AuthorizationFilterContext CreateContext()
    {
        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor());
        return new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
    }

    [Fact(DisplayName = "AdminRead mode always allows the request.")]
    public async Task AdminRead_AlwaysPasses()
    {
        // Arrange
        var policy = new Mock<ISiteFederationPolicy>();
        var identity = new Mock<IFederationInstanceIdentity>();
        var filter = new RequireFederationEnabledFilter(
            FederationGateMode.AdminRead,
            policy.Object,
            identity.Object);
        var context = CreateContext();

        // Act
        await filter.OnAuthorizationAsync(context);

        // Assert
        context.Result.ShouldBeNull();
        policy.Verify(p => p.GetSnapshotAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Mutation mode returns 403 when federation is disabled.")]
    public async Task Mutation_WhenDisabled_Returns403()
    {
        // Arrange
        var policy = new Mock<ISiteFederationPolicy>();
        policy.Setup(p => p.GetSnapshotAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(SiteFederationPolicySnapshot.Disabled);
        var identity = new Mock<IFederationInstanceIdentity>();
        var filter = new RequireFederationEnabledFilter(
            FederationGateMode.Mutation,
            policy.Object,
            identity.Object);
        var context = CreateContext();

        // Act
        await filter.OnAuthorizationAsync(context);

        // Assert
        var result = context.Result.ShouldBeOfType<ObjectResult>();
        result.StatusCode.ShouldBe(StatusCodes.Status403Forbidden);
    }

    [Fact(DisplayName = "Peer mode returns 503 when federation is enabled but identity is incomplete.")]
    public async Task Peer_WhenIdentityIncomplete_Returns503()
    {
        // Arrange
        var policy = new Mock<ISiteFederationPolicy>();
        policy.Setup(p => p.GetSnapshotAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SiteFederationPolicySnapshot(
                Guid.NewGuid(),
                true,
                "Node",
                "",
                ""));
        var identity = new Mock<IFederationInstanceIdentity>();
        identity.Setup(i => i.GetSnapshotAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FederationInstanceIdentitySnapshot(
                Guid.NewGuid(),
                "Node",
                "",
                "",
                "",
                false));
        var filter = new RequireFederationEnabledFilter(
            FederationGateMode.Peer,
            policy.Object,
            identity.Object);
        var context = CreateContext();

        // Act
        await filter.OnAuthorizationAsync(context);

        // Assert
        var result = context.Result.ShouldBeOfType<ObjectResult>();
        result.StatusCode.ShouldBe(StatusCodes.Status503ServiceUnavailable);
    }

    [Fact(DisplayName = "Peer mode returns 403 when federation is disabled.")]
    public async Task Peer_WhenDisabled_Returns403()
    {
        // Arrange
        var policy = new Mock<ISiteFederationPolicy>();
        policy.Setup(p => p.GetSnapshotAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(SiteFederationPolicySnapshot.Disabled);
        var identity = new Mock<IFederationInstanceIdentity>();
        var filter = new RequireFederationEnabledFilter(
            FederationGateMode.Peer,
            policy.Object,
            identity.Object);
        var context = CreateContext();

        // Act
        await filter.OnAuthorizationAsync(context);

        // Assert
        var result = context.Result.ShouldBeOfType<ObjectResult>();
        result.StatusCode.ShouldBe(StatusCodes.Status403Forbidden);
    }
}