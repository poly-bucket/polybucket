using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Reports.ResolveReport.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Reports;

public class ResolveReportControllerTests
{
    [Fact]
    public void Controller_ShouldHaveApiControllerAndRoute()
    {
        // Arrange
        var controllerType = typeof(ResolveReportController);
        // Act
        var apiAttr = controllerType.GetCustomAttribute<ApiControllerAttribute>();
        var routeAttr = controllerType.GetCustomAttribute<RouteAttribute>();
        // Assert
        apiAttr.ShouldNotBeNull();
        routeAttr.ShouldNotBeNull();
    }
}
