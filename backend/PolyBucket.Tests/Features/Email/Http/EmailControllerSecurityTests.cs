using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PolyBucket.Api.Extensions;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Email.GetEmailOutbox.Http;
using PolyBucket.Api.Features.Email.GetEmailSettings.Http;
using PolyBucket.Api.Features.Email.PreviewEmailTemplate.Http;
using PolyBucket.Api.Features.Email.RetryEmailMessage.Http;
using PolyBucket.Api.Features.Email.TestEmailConfiguration.Http;
using PolyBucket.Api.Features.Email.UpdateEmailSettings.Http;
using Shouldly;
using Xunit;

namespace PolyBucket.Tests.Features.Email.Http;

public class EmailControllerSecurityTests
{
    public static IEnumerable<object[]> ControllerTypes()
    {
        return new List<object[]>
        {
            new object[] { typeof(GetEmailSettingsController) },
            new object[] { typeof(UpdateEmailSettingsController) },
            new object[] { typeof(TestEmailConfigurationController) },
            new object[] { typeof(GetEmailOutboxController) },
            new object[] { typeof(RetryEmailMessageController) },
            new object[] { typeof(PreviewEmailTemplateController) }
        };
    }

    [Theory(DisplayName = "Every email admin controller requires authentication and the system settings permission.")]
    [MemberData(nameof(ControllerTypes))]
    public void Controller_ShouldRequireSystemSettingsPermission(Type controllerType)
    {
        // Arrange
        var permissionsField = typeof(RequirePermissionAttribute).GetField("_permissions", BindingFlags.NonPublic | BindingFlags.Instance);

        // Act
        var apiAttr = controllerType.GetCustomAttribute<ApiControllerAttribute>();
        var authorizeAttr = controllerType.GetCustomAttribute<AuthorizeAttribute>();
        var permissionAttr = controllerType.GetCustomAttribute<RequirePermissionAttribute>();

        // Assert
        apiAttr.ShouldNotBeNull();
        authorizeAttr.ShouldNotBeNull();
        permissionAttr.ShouldNotBeNull();
        var permissions = (string[])permissionsField!.GetValue(permissionAttr!)!;
        permissions.ShouldContain(PermissionConstants.ADMIN_SYSTEM_SETTINGS);
    }

    [Fact(DisplayName = "The test email endpoint uses the strict rate limiting policy to prevent relay abuse.")]
    public void TestEmailConfiguration_ShouldBeRateLimited()
    {
        // Arrange
        var method = typeof(TestEmailConfigurationController).GetMethod(nameof(TestEmailConfigurationController.TestEmailConfiguration));

        // Act
        var rateLimit = method!.GetCustomAttribute<EnableRateLimitingAttribute>();

        // Assert
        rateLimit.ShouldNotBeNull();
        rateLimit!.PolicyName.ShouldBe(RequestSecurityServiceCollectionExtensions.AuthStrictPolicy);
    }
}
