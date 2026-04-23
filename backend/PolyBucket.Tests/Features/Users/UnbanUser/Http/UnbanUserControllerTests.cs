using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Users.UnbanUser.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Users.UnbanUser.Http;

public class UnbanUserControllerTests
{
    [Fact]
    public void Controller_ShouldHaveApiControllerAndRoute()
    {
        var controllerType = typeof(UnbanUserController);
        var apiAttr = controllerType.GetCustomAttribute<ApiControllerAttribute>();
        var routeAttr = controllerType.GetCustomAttribute<RouteAttribute>();
        apiAttr.ShouldNotBeNull();
        routeAttr.ShouldNotBeNull();
    }
}
