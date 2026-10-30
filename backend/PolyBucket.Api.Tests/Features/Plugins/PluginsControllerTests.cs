using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Plugins.GetPlugins.Http;
using PolyBucket.Api.Features.Plugins.ReloadPlugins.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Plugins;

public class PluginsControllerTests
{
    public static IEnumerable<object[]> ControllerTypes()
    {
        return new List<object[]>
        {
            new object[] { typeof(ReloadPluginsController) },
            new object[] { typeof(GetPluginsController) }
        };
    }

    [Theory]
    [MemberData(nameof(ControllerTypes))]
    public void Controller_ShouldHaveApiControllerAndRoute(Type controllerType)
    {
        // Arrange

        // Act
        var apiAttr = controllerType.GetCustomAttribute<ApiControllerAttribute>();
        var routeAttr = controllerType.GetCustomAttribute<RouteAttribute>();

        // Assert
        apiAttr.ShouldNotBeNull();
        routeAttr.ShouldNotBeNull();
    }
}
