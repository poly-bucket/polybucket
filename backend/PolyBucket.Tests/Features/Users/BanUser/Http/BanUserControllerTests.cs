using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Users.BanUser.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Users.BanUser.Http;

public class BanUserControllerTests
{
    [Fact(DisplayName = "When inspecting the ban user controller, the controller has the ApiController and Route attributes applied.")]
    public void Controller_ShouldHaveApiControllerAndRoute()
    {
        var controllerType = typeof(BanUserController);
        var apiAttr = controllerType.GetCustomAttribute<ApiControllerAttribute>();
        var routeAttr = controllerType.GetCustomAttribute<RouteAttribute>();
        apiAttr.ShouldNotBeNull();
        routeAttr.ShouldNotBeNull();
    }
}
