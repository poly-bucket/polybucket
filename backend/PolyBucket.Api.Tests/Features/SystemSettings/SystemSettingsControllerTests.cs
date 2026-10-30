using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using Xunit;

namespace PolyBucket.Api.Tests.Features.SystemSettings;

public class SystemSettingsControllerTests
{
    public static IEnumerable<object[]> ControllerTypes()
    {
        return new List<object[]>
        {
                        new object[] { typeof(PolyBucket.Api.Features.SystemSettings.ApplyStorageSetup.Http.ApplyStorageSetupController) },
            new object[] { typeof(PolyBucket.Api.Features.SystemSettings.CheckFirstTimeSetup.Http.CheckFirstTimeSetupController) },
            new object[] { typeof(PolyBucket.Api.Features.SystemSettings.CompleteFirstTimeSetup.Http.CompleteFirstTimeSetupController) },
            new object[] { typeof(PolyBucket.Api.Features.SystemSettings.GetActiveExtensibleTheme.Http.GetActiveExtensibleThemeController) },
            new object[] { typeof(PolyBucket.Api.Features.SystemSettings.GetActiveExtensibleThemeComponents.Http.GetActiveExtensibleThemeComponentsController) },
            new object[] { typeof(PolyBucket.Api.Features.SystemSettings.GetAuthenticationSettings.Http.GetAuthenticationSettingsController) },
            new object[] { typeof(PolyBucket.Api.Features.SystemSettings.GetExtensibleThemeComponents.Http.GetExtensibleThemeComponentsController) },
            new object[] { typeof(PolyBucket.Api.Features.SystemSettings.GetExtensibleThemeConfiguration.Http.GetExtensibleThemeConfigurationController) },
            new object[] { typeof(PolyBucket.Api.Features.SystemSettings.GetExtensibleThemePlugins.Http.GetExtensibleThemePluginsController) },
            new object[] { typeof(PolyBucket.Api.Features.SystemSettings.GetExtensibleThemes.Http.GetExtensibleThemesController) },
            new object[] { typeof(PolyBucket.Api.Features.SystemSettings.GetFileSettings.Http.GetFileSettingsController) },
            new object[] { typeof(PolyBucket.Api.Features.SystemSettings.GetFontAwesomeProEnabled.Http.GetFontAwesomeProEnabledController) },
            new object[] { typeof(PolyBucket.Api.Features.SystemSettings.GetFontAwesomeSettings.Http.GetFontAwesomeSettingsController) },
            new object[] { typeof(PolyBucket.Api.Features.SystemSettings.GetModelConfigurationSettings.Http.GetModelConfigurationSettingsController) },
            new object[] { typeof(PolyBucket.Api.Features.SystemSettings.GetSiteModelSettings.Http.GetSiteModelSettingsController) },
            new object[] { typeof(PolyBucket.Api.Features.SystemSettings.GetSiteSettings.Http.GetSiteSettingsController) },
            new object[] { typeof(PolyBucket.Api.Features.SystemSettings.GetStorageProviders.Http.GetStorageProvidersController) },
            new object[] { typeof(PolyBucket.Api.Features.SystemSettings.GetStorageSettings.Http.GetStorageSettingsController) },
            new object[] { typeof(PolyBucket.Api.Features.SystemSettings.GetThemeSettings.Http.GetThemeSettingsController) },
            new object[] { typeof(PolyBucket.Api.Features.SystemSettings.GetTokenSettings.Http.GetTokenSettingsController) },
            new object[] { typeof(PolyBucket.Api.Features.SystemSettings.ResetExtensibleTheme.Http.ResetExtensibleThemeController) },
            new object[] { typeof(PolyBucket.Api.Features.SystemSettings.SetActiveExtensibleTheme.Http.SetActiveExtensibleThemeController) },
            new object[] { typeof(PolyBucket.Api.Features.SystemSettings.SkipEmailSetup.Http.SkipEmailSetupController) },
            new object[] { typeof(PolyBucket.Api.Features.SystemSettings.TestFontAwesomeLicense.Http.TestFontAwesomeLicenseController) },
            new object[] { typeof(PolyBucket.Api.Features.SystemSettings.TestStorageConfiguration.Http.TestStorageConfigurationController) },
            new object[] { typeof(PolyBucket.Api.Features.SystemSettings.UpdateAuthenticationSettings.Http.UpdateAuthenticationSettingsController) },
            new object[] { typeof(PolyBucket.Api.Features.SystemSettings.UpdateExtensibleThemeConfiguration.Http.UpdateExtensibleThemeConfigurationController) },
            new object[] { typeof(PolyBucket.Api.Features.SystemSettings.UpdateFileSettings.Http.UpdateFileSettingsController) },
            new object[] { typeof(PolyBucket.Api.Features.SystemSettings.UpdateFontAwesomeSettings.Http.UpdateFontAwesomeSettingsController) },
            new object[] { typeof(PolyBucket.Api.Features.SystemSettings.UpdateModelConfigurationSettings.Http.UpdateModelConfigurationSettingsController) },
            new object[] { typeof(PolyBucket.Api.Features.SystemSettings.UpdateSiteModelSettings.Http.UpdateSiteModelSettingsController) },
            new object[] { typeof(PolyBucket.Api.Features.SystemSettings.UpdateSiteSettings.Http.UpdateSiteSettingsController) },
            new object[] { typeof(PolyBucket.Api.Features.SystemSettings.UpdateStorageSettings.Http.UpdateStorageSettingsController) },
            new object[] { typeof(PolyBucket.Api.Features.SystemSettings.UpdateThemeSettings.Http.UpdateThemeSettingsController) },
            new object[] { typeof(PolyBucket.Api.Features.SystemSettings.UpdateTokenSettings.Http.UpdateTokenSettingsController) },
            new object[] { typeof(PolyBucket.Api.Features.SystemSettings.ValidateExtensibleThemeConfiguration.Http.ValidateExtensibleThemeConfigurationController) },
        };
    }

    [Theory]
    [MemberData(nameof(ControllerTypes))]
    public void Controller_ShouldHaveApiControllerAndRoute(Type controllerType)
    {
        // Arrange & Act
        var apiAttr = controllerType.GetCustomAttribute<ApiControllerAttribute>();
        var routeAttr = controllerType.GetCustomAttribute<RouteAttribute>();

        // Assert
        apiAttr.ShouldNotBeNull();
        routeAttr.ShouldNotBeNull();
    }
}
