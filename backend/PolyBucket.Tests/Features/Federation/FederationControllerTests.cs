using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Federation.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Federation;

public class FederationControllerTests : IDisposable
{
    public static IEnumerable<object[]> ControllerTypes()
    {
        return new List<object[]>
        {
            new object[] { typeof(GetFederatedInstancesController) },
            new object[] { typeof(GetFederatedInstanceController) },
            new object[] { typeof(CreateFederatedInstanceController) },
            new object[] { typeof(UpdateFederatedInstanceController) },
            new object[] { typeof(DeleteFederatedInstanceController) },
            new object[] { typeof(GetFederatedModelsController) },
            new object[] { typeof(ImportFederatedModelController) },
            new object[] { typeof(GetFederationHealthController) }
        };
    }

    [Theory]
    [MemberData(nameof(ControllerTypes))]
    public void Controller_ShouldHaveApiControllerAndRoute(Type controllerType)
    {
        var apiAttr = controllerType.GetCustomAttribute<ApiControllerAttribute>();
        apiAttr.ShouldNotBeNull();

        var routeAttr = controllerType.GetCustomAttribute<RouteAttribute>();
        routeAttr.ShouldNotBeNull();
    }

    [Fact]
    public void GetFederatedInstancesController_ShouldHaveCorrectRoute()
    {
        var routeAttr = typeof(GetFederatedInstancesController).GetCustomAttribute<RouteAttribute>();
        routeAttr.ShouldNotBeNull();
        routeAttr.Template.ShouldBe("api/federation/instances");
    }

    [Fact]
    public void GetFederatedModelsController_ShouldHaveCorrectRoute()
    {
        var routeAttr = typeof(GetFederatedModelsController).GetCustomAttribute<RouteAttribute>();
        routeAttr.ShouldNotBeNull();
        routeAttr.Template.ShouldBe("api/federation/models");
    }

    [Fact]
    public void GetFederationHealthController_ShouldHaveCorrectRoute()
    {
        var routeAttr = typeof(GetFederationHealthController).GetCustomAttribute<RouteAttribute>();
        routeAttr.ShouldNotBeNull();
        routeAttr.Template.ShouldBe("api/federation/health");
    }

    public void Dispose()
    {
        // No resources to dispose in this test class
    }
}

