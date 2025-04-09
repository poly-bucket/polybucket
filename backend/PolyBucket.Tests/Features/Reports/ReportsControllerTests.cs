using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Reports.Commands;
using PolyBucket.Api.Features.Reports.Queries;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Reports;

public class ReportsControllerTests
{
    public static IEnumerable<object[]> ControllerTypes()
    {
        return new List<object[]>
        {
            new object[] { typeof(ResolveReportController) },
            new object[] { typeof(SubmitReportController) },
            new object[] { typeof(GetReportController) },
            new object[] { typeof(GetReportsForTargetController) },
            new object[] { typeof(GetUnresolvedReportsController) }
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