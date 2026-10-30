using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Printers.GetPrinters.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.Printers;

public class PrintersControllerTests
{
    public static IEnumerable<object[]> ControllerTypes()
    {
        return new List<object[]>
        {
            new object[] { typeof(GetPrintersController) }
        };
    }

    [Theory]
    [MemberData(nameof(ControllerTypes))]
    public void Controller_ShouldHaveApiControllerAndRoute(Type controllerType)
    {
        // Ensure ApiController attribute exists
        var apiAttr = controllerType.GetCustomAttribute<ApiControllerAttribute>();
        apiAttr.ShouldNotBeNull();

        // Ensure RouteAttribute exists
        var routeAttr = controllerType.GetCustomAttribute<RouteAttribute>();
        routeAttr.ShouldNotBeNull();
    }
} 