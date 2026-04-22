using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Reports.GetAllReports.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Reports;

public class GetAllReportsControllerTests
{
    [Fact]
    public void Controller_ShouldHaveApiControllerAndRoute()
    {
        // Arrange
        var controllerType = typeof(GetAllReportsController);
        // Act
        var apiAttr = controllerType.GetCustomAttribute<ApiControllerAttribute>();
        var routeAttr = controllerType.GetCustomAttribute<RouteAttribute>();
        // Assert
        apiAttr.ShouldNotBeNull();
        routeAttr.ShouldNotBeNull();
    }
}
