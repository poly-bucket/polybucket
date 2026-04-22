using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Reports.SubmitReport.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Reports;

public class SubmitReportControllerTests
{
    [Fact]
    public void Controller_ShouldHaveApiControllerAndRoute()
    {
        // Arrange
        var controllerType = typeof(SubmitReportController);
        // Act
        var apiAttr = controllerType.GetCustomAttribute<ApiControllerAttribute>();
        var routeAttr = controllerType.GetCustomAttribute<RouteAttribute>();
        // Assert
        apiAttr.ShouldNotBeNull();
        routeAttr.ShouldNotBeNull();
    }
}
