using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Filaments.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Filaments;

public class FilamentsControllerTests
{
    public static IEnumerable<object[]> ControllerTypes()
    {
        return new List<object[]>
        {
            new object[] { typeof(CreateFilamentController) },
            new object[] { typeof(DeleteFilamentController) },
            new object[] { typeof(GetAllFilamentsController) },
            new object[] { typeof(GetFilamentByIdController) },
            new object[] { typeof(UpdateFilamentController) }
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