using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ModelModeration.ApproveModel.Http;
using PolyBucket.Api.Features.ModelModeration.GetModelsAwaitingModeration.Http;
using PolyBucket.Api.Features.ModelModeration.GetModerationSettings.Http;
using PolyBucket.Api.Features.ModelModeration.ModeratorEditModel.Http;
using PolyBucket.Api.Features.ModelModeration.RejectModel.Http;
using PolyBucket.Api.Features.ModelModeration.UpdateModerationSettings.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.ModelModeration;

public class ModelModerationControllerTests
{
    public static IEnumerable<object[]> ControllerTypes()
    {
        return new List<object[]>
        {
            new object[] { typeof(ApproveModelController), PermissionConstants.MODERATION_APPROVE_MODELS },
            new object[] { typeof(GetModelsAwaitingModerationController), PermissionConstants.MODERATION_VIEW_QUEUE },
            new object[] { typeof(GetModerationSettingsController), PermissionConstants.MODERATION_VIEW_QUEUE },
            new object[] { typeof(RejectModelController), PermissionConstants.MODERATION_REJECT_MODELS },
            new object[] { typeof(UpdateModerationSettingsController), PermissionConstants.ADMIN_SYSTEM_SETTINGS },
            new object[] { typeof(ModeratorEditModelController), PermissionConstants.MODERATION_EDIT_MODELS }
        };
    }

    [Theory]
    [MemberData(nameof(ControllerTypes))]
    public void Controller_ShouldHaveApiControllerRouteAndPermission(Type controllerType, string permission)
    {
        // Arrange
        // Act
        var apiAttr = controllerType.GetCustomAttribute<ApiControllerAttribute>();
        var routeAttr = controllerType.GetCustomAttribute<RouteAttribute>();
        var permissionAttr = controllerType.GetCustomAttribute<RequirePermissionAttribute>();

        // Assert
        apiAttr.ShouldNotBeNull();
        routeAttr.ShouldNotBeNull();
        permissionAttr.ShouldNotBeNull();
        var permissionsField = typeof(RequirePermissionAttribute).GetField("_permissions", BindingFlags.NonPublic | BindingFlags.Instance);
        var permissions = (string[])permissionsField!.GetValue(permissionAttr!)!;
        permissions.ShouldContain(permission);
    }
}
