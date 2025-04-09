using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Collections.CreateCollection.Http;
using PolyBucket.Api.Features.Collections.DeleteCollection.Http;
using PolyBucket.Api.Features.Collections.GetCollectionById.Http;
using PolyBucket.Api.Features.Collections.UpdateCollection.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Collections;

public class CollectionsControllerTests
{
    public static IEnumerable<object[]> ControllerTypes()
    {
        return new List<object[]>
        {
            new object[] { typeof(CreateCollectionController) },
            new object[] { typeof(DeleteCollectionController) },
            new object[] { typeof(GetCollectionByIdController) },
            new object[] { typeof(UpdateCollectionController) }
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