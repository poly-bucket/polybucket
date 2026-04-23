using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Users.GetBannedUsers.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Users.GetBannedUsers.Http;

public class GetBannedUsersControllerTests
{
    [Fact]
    public void Controller_ShouldHaveApiControllerAndRoute()
    {
        var controllerType = typeof(GetBannedUsersController);
        var apiAttr = controllerType.GetCustomAttribute<ApiControllerAttribute>();
        var routeAttr = controllerType.GetCustomAttribute<RouteAttribute>();
        apiAttr.ShouldNotBeNull();
        routeAttr.ShouldNotBeNull();
    }
}
