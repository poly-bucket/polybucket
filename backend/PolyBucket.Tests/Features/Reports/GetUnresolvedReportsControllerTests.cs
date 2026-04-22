using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Reports.GetUnresolvedReports.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Reports;

public class GetUnresolvedReportsControllerTests
{
    [Fact]
    public void Controller_ShouldHaveApiControllerAndRoute()
    {
        // Arrange
        var controllerType = typeof(GetUnresolvedReportsController);
        // Act
        var apiAttr = controllerType.GetCustomAttribute<ApiControllerAttribute>();
        var routeAttr = controllerType.GetCustomAttribute<RouteAttribute>();
        // Assert
        apiAttr.ShouldNotBeNull();
        routeAttr.ShouldNotBeNull();
    }
}
