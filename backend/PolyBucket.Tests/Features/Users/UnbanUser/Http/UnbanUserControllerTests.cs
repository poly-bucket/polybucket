using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Users.UnbanUser.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Users.UnbanUser.Http;

public class UnbanUserControllerTests
{
    [Fact(DisplayName = "When inspecting the unban user controller, the controller has the ApiController and Route attributes applied.")]
    public void Controller_ShouldHaveApiControllerAndRoute()
    {
        var controllerType = typeof(UnbanUserController);
        var apiAttr = controllerType.GetCustomAttribute<ApiControllerAttribute>();
        var routeAttr = controllerType.GetCustomAttribute<RouteAttribute>();
        apiAttr.ShouldNotBeNull();
        routeAttr.ShouldNotBeNull();
    }
}
