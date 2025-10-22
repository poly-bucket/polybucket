using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Users.Queries;
using PolyBucket.Api.Features.Users.UpdateUserSettings.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.UserSettings;

public class UserSettingsControllerTests
{
    public static IEnumerable<object[]> ControllerTypes()
    {
        return new List<object[]>
        {
            new object[] { typeof(GetUserSettingsController) },
            new object[] { typeof(UpdateUserSettingsController) }
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

    [Fact]
    public void UpdateUserSettingsRequest_ShouldHaveDashboardLayoutProperties()
    {
        // Arrange
        var requestType = typeof(UpdateUserSettingsRequest);

        // Assert
        requestType.GetProperty("DashboardViewType").ShouldNotBeNull();
        requestType.GetProperty("CardSize").ShouldNotBeNull();
        requestType.GetProperty("CardSpacing").ShouldNotBeNull();
        requestType.GetProperty("GridColumns").ShouldNotBeNull();
    }

    [Fact]
    public void UpdateUserSettingsRequest_PropertiesShouldBeNullable()
    {
        // Arrange
        var requestType = typeof(UpdateUserSettingsRequest);

        // Assert
        requestType.GetProperty("DashboardViewType")?.PropertyType.ShouldBe(typeof(string));
        requestType.GetProperty("CardSize")?.PropertyType.ShouldBe(typeof(string));
        requestType.GetProperty("CardSpacing")?.PropertyType.ShouldBe(typeof(string));
        requestType.GetProperty("GridColumns")?.PropertyType.ShouldBe(typeof(int?));
    }
} 