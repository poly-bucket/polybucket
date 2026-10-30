using Microsoft.Extensions.DependencyInjection;

namespace PolyBucket.Api.Features.SystemSettings;

public static class SystemSettingsDependencyInjectionMapping
{
    public static IServiceCollection AddSystemSettingsFeature(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddScoped<Services.ISitePrivacyState, Services.SitePrivacyState>();

        services.AddTransient<GetAuthenticationSettings.Repository.AuthenticationSettingsPersistence>();
        services.AddTransient<GetAuthenticationSettings.Repository.IGetAuthenticationSettingsRepository>(sp => sp.GetRequiredService<GetAuthenticationSettings.Repository.AuthenticationSettingsPersistence>());
        services.AddTransient<UpdateAuthenticationSettings.Repository.IUpdateAuthenticationSettingsRepository>(sp => sp.GetRequiredService<GetAuthenticationSettings.Repository.AuthenticationSettingsPersistence>());

        services.AddTransient<GetTokenSettings.Repository.TokenSettingsPersistence>();
        services.AddTransient<GetTokenSettings.Repository.IGetTokenSettingsRepository>(sp => sp.GetRequiredService<GetTokenSettings.Repository.TokenSettingsPersistence>());
        services.AddTransient<UpdateTokenSettings.Repository.IUpdateTokenSettingsRepository>(sp => sp.GetRequiredService<GetTokenSettings.Repository.TokenSettingsPersistence>());

        services.AddTransient<Services.IAuthenticationSettingsService, Services.AuthenticationSettingsService>();
        services.AddTransient<Services.ITokenSettingsService, Services.TokenSettingsService>();
        services.AddTransient<Repository.IFontAwesomeSettingsRepository, Repository.FontAwesomeSettingsRepository>();

        Register<GetAuthenticationSettings.Domain.IGetAuthenticationSettingsService, GetAuthenticationSettings.Domain.GetAuthenticationSettingsService,
            GetAuthenticationSettings.Repository.IGetAuthenticationSettingsRepository, GetAuthenticationSettings.Repository.AuthenticationSettingsPersistence>(services);
        Register<UpdateAuthenticationSettings.Domain.IUpdateAuthenticationSettingsService, UpdateAuthenticationSettings.Domain.UpdateAuthenticationSettingsService,
            UpdateAuthenticationSettings.Repository.IUpdateAuthenticationSettingsRepository, GetAuthenticationSettings.Repository.AuthenticationSettingsPersistence>(services);
        Register<GetTokenSettings.Domain.IGetTokenSettingsService, GetTokenSettings.Domain.GetTokenSettingsService,
            GetTokenSettings.Repository.IGetTokenSettingsRepository, GetTokenSettings.Repository.TokenSettingsPersistence>(services);
        Register<UpdateTokenSettings.Domain.IUpdateTokenSettingsService, UpdateTokenSettings.Domain.UpdateTokenSettingsService,
            UpdateTokenSettings.Repository.IUpdateTokenSettingsRepository, GetTokenSettings.Repository.TokenSettingsPersistence>(services);

        Register<GetFontAwesomeSettings.Domain.IGetFontAwesomeSettingsService, GetFontAwesomeSettings.Domain.GetFontAwesomeSettingsService,
            GetFontAwesomeSettings.Repository.IGetFontAwesomeSettingsRepository, GetFontAwesomeSettings.Repository.GetFontAwesomeSettingsRepository>(services);
        Register<UpdateFontAwesomeSettings.Domain.IUpdateFontAwesomeSettingsService, UpdateFontAwesomeSettings.Domain.UpdateFontAwesomeSettingsService,
            UpdateFontAwesomeSettings.Repository.IUpdateFontAwesomeSettingsRepository, UpdateFontAwesomeSettings.Repository.UpdateFontAwesomeSettingsRepository>(services);
        Register<TestFontAwesomeLicense.Domain.ITestFontAwesomeLicenseService, TestFontAwesomeLicense.Domain.TestFontAwesomeLicenseService,
            TestFontAwesomeLicense.Repository.ITestFontAwesomeLicenseRepository, TestFontAwesomeLicense.Repository.TestFontAwesomeLicenseRepository>(services);
        Register<GetFontAwesomeProEnabled.Domain.IGetFontAwesomeProEnabledService, GetFontAwesomeProEnabled.Domain.GetFontAwesomeProEnabledService,
            GetFontAwesomeProEnabled.Repository.IGetFontAwesomeProEnabledRepository, GetFontAwesomeProEnabled.Repository.GetFontAwesomeProEnabledRepository>(services);

        Register<GetFileSettings.Domain.IGetFileSettingsService, GetFileSettings.Domain.GetFileSettingsService,
            GetFileSettings.Repository.IGetFileSettingsRepository, GetFileSettings.Repository.GetFileSettingsRepository>(services);
        Register<UpdateFileSettings.Domain.IUpdateFileSettingsService, UpdateFileSettings.Domain.UpdateFileSettingsService,
            UpdateFileSettings.Repository.IUpdateFileSettingsRepository, UpdateFileSettings.Repository.UpdateFileSettingsRepository>(services);
        Register<GetThemeSettings.Domain.IGetThemeSettingsService, GetThemeSettings.Domain.GetThemeSettingsService,
            GetThemeSettings.Repository.IGetThemeSettingsRepository, GetThemeSettings.Repository.GetThemeSettingsRepository>(services);
        Register<UpdateThemeSettings.Domain.IUpdateThemeSettingsService, UpdateThemeSettings.Domain.UpdateThemeSettingsService,
            UpdateThemeSettings.Repository.IUpdateThemeSettingsRepository, UpdateThemeSettings.Repository.UpdateThemeSettingsRepository>(services);
        Register<GetSiteModelSettings.Domain.IGetSiteModelSettingsService, GetSiteModelSettings.Domain.GetSiteModelSettingsService,
            GetSiteModelSettings.Repository.IGetSiteModelSettingsRepository, GetSiteModelSettings.Repository.GetSiteModelSettingsRepository>(services);
        Register<UpdateSiteModelSettings.Domain.IUpdateSiteModelSettingsService, UpdateSiteModelSettings.Domain.UpdateSiteModelSettingsService,
            UpdateSiteModelSettings.Repository.IUpdateSiteModelSettingsRepository, UpdateSiteModelSettings.Repository.UpdateSiteModelSettingsRepository>(services);
        Register<GetModelConfigurationSettings.Domain.IGetModelConfigurationSettingsService, GetModelConfigurationSettings.Domain.GetModelConfigurationSettingsService,
            GetModelConfigurationSettings.Repository.IGetModelConfigurationSettingsRepository, GetModelConfigurationSettings.Repository.GetModelConfigurationSettingsRepository>(services);
        Register<UpdateModelConfigurationSettings.Domain.IUpdateModelConfigurationSettingsService, UpdateModelConfigurationSettings.Domain.UpdateModelConfigurationSettingsService,
            UpdateModelConfigurationSettings.Repository.IUpdateModelConfigurationSettingsRepository, UpdateModelConfigurationSettings.Repository.UpdateModelConfigurationSettingsRepository>(services);
        Register<CheckFirstTimeSetup.Domain.ICheckFirstTimeSetupService, CheckFirstTimeSetup.Domain.CheckFirstTimeSetupService,
            CheckFirstTimeSetup.Repository.ICheckFirstTimeSetupRepository, CheckFirstTimeSetup.Repository.CheckFirstTimeSetupRepository>(services);
        Register<GetSiteSettings.Domain.IGetSiteSettingsService, GetSiteSettings.Domain.GetSiteSettingsService,
            GetSiteSettings.Repository.IGetSiteSettingsRepository, GetSiteSettings.Repository.GetSiteSettingsRepository>(services);
        Register<UpdateSiteSettings.Domain.IUpdateSiteSettingsService, UpdateSiteSettings.Domain.UpdateSiteSettingsService,
            UpdateSiteSettings.Repository.IUpdateSiteSettingsRepository, UpdateSiteSettings.Repository.UpdateSiteSettingsRepository>(services);
        Register<CompleteFirstTimeSetup.Domain.ICompleteFirstTimeSetupService, CompleteFirstTimeSetup.Domain.CompleteFirstTimeSetupService,
            CompleteFirstTimeSetup.Repository.ICompleteFirstTimeSetupRepository, CompleteFirstTimeSetup.Repository.CompleteFirstTimeSetupRepository>(services);
        Register<ApplyStorageSetup.Domain.IApplyStorageSetupService, ApplyStorageSetup.Domain.ApplyStorageSetupService,
            ApplyStorageSetup.Repository.IApplyStorageSetupRepository, ApplyStorageSetup.Repository.ApplyStorageSetupRepository>(services);
        Register<SkipEmailSetup.Domain.ISkipEmailSetupService, SkipEmailSetup.Domain.SkipEmailSetupService,
            SkipEmailSetup.Repository.ISkipEmailSetupRepository, SkipEmailSetup.Repository.SkipEmailSetupRepository>(services);

        Register<GetStorageProviders.Domain.IGetStorageProvidersService, GetStorageProviders.Domain.GetStorageProvidersService,
            GetStorageProviders.Repository.IGetStorageProvidersRepository, GetStorageProviders.Repository.GetStorageProvidersRepository>(services);
        Register<GetStorageSettings.Domain.IGetStorageSettingsService, GetStorageSettings.Domain.GetStorageSettingsService,
            GetStorageSettings.Repository.IGetStorageSettingsRepository, GetStorageSettings.Repository.GetStorageSettingsRepository>(services);
        Register<UpdateStorageSettings.Domain.IUpdateStorageSettingsService, UpdateStorageSettings.Domain.UpdateStorageSettingsService,
            UpdateStorageSettings.Repository.IUpdateStorageSettingsRepository, UpdateStorageSettings.Repository.UpdateStorageSettingsRepository>(services);
        Register<TestStorageConfiguration.Domain.ITestStorageConfigurationService, TestStorageConfiguration.Domain.TestStorageConfigurationService,
            TestStorageConfiguration.Repository.ITestStorageConfigurationRepository, TestStorageConfiguration.Repository.TestStorageConfigurationRepository>(services);

        RegisterExtensibleTheme<GetExtensibleThemes.Domain.IGetExtensibleThemesService, GetExtensibleThemes.Domain.GetExtensibleThemesService,
            GetExtensibleThemes.Repository.IGetExtensibleThemesRepository, GetExtensibleThemes.Repository.GetExtensibleThemesRepository>(services);
        RegisterExtensibleTheme<GetActiveExtensibleTheme.Domain.IGetActiveExtensibleThemeService, GetActiveExtensibleTheme.Domain.GetActiveExtensibleThemeService,
            GetActiveExtensibleTheme.Repository.IGetActiveExtensibleThemeRepository, GetActiveExtensibleTheme.Repository.GetActiveExtensibleThemeRepository>(services);
        RegisterExtensibleTheme<SetActiveExtensibleTheme.Domain.ISetActiveExtensibleThemeService, SetActiveExtensibleTheme.Domain.SetActiveExtensibleThemeService,
            SetActiveExtensibleTheme.Repository.ISetActiveExtensibleThemeRepository, SetActiveExtensibleTheme.Repository.SetActiveExtensibleThemeRepository>(services);
        RegisterExtensibleTheme<GetActiveExtensibleThemeComponents.Domain.IGetActiveExtensibleThemeComponentsService, GetActiveExtensibleThemeComponents.Domain.GetActiveExtensibleThemeComponentsService,
            GetActiveExtensibleThemeComponents.Repository.IGetActiveExtensibleThemeComponentsRepository, GetActiveExtensibleThemeComponents.Repository.GetActiveExtensibleThemeComponentsRepository>(services);
        RegisterExtensibleTheme<GetExtensibleThemeComponents.Domain.IGetExtensibleThemeComponentsService, GetExtensibleThemeComponents.Domain.GetExtensibleThemeComponentsService,
            GetExtensibleThemeComponents.Repository.IGetExtensibleThemeComponentsRepository, GetExtensibleThemeComponents.Repository.GetExtensibleThemeComponentsRepository>(services);
        RegisterExtensibleTheme<GetExtensibleThemeConfiguration.Domain.IGetExtensibleThemeConfigurationService, GetExtensibleThemeConfiguration.Domain.GetExtensibleThemeConfigurationService,
            GetExtensibleThemeConfiguration.Repository.IGetExtensibleThemeConfigurationRepository, GetExtensibleThemeConfiguration.Repository.GetExtensibleThemeConfigurationRepository>(services);
        RegisterExtensibleTheme<UpdateExtensibleThemeConfiguration.Domain.IUpdateExtensibleThemeConfigurationService, UpdateExtensibleThemeConfiguration.Domain.UpdateExtensibleThemeConfigurationService,
            UpdateExtensibleThemeConfiguration.Repository.IUpdateExtensibleThemeConfigurationRepository, UpdateExtensibleThemeConfiguration.Repository.UpdateExtensibleThemeConfigurationRepository>(services);
        RegisterExtensibleTheme<ValidateExtensibleThemeConfiguration.Domain.IValidateExtensibleThemeConfigurationService, ValidateExtensibleThemeConfiguration.Domain.ValidateExtensibleThemeConfigurationService,
            ValidateExtensibleThemeConfiguration.Repository.IValidateExtensibleThemeConfigurationRepository, ValidateExtensibleThemeConfiguration.Repository.ValidateExtensibleThemeConfigurationRepository>(services);
        RegisterExtensibleTheme<ResetExtensibleTheme.Domain.IResetExtensibleThemeService, ResetExtensibleTheme.Domain.ResetExtensibleThemeService,
            ResetExtensibleTheme.Repository.IResetExtensibleThemeRepository, ResetExtensibleTheme.Repository.ResetExtensibleThemeRepository>(services);
        RegisterExtensibleTheme<GetExtensibleThemePlugins.Domain.IGetExtensibleThemePluginsService, GetExtensibleThemePlugins.Domain.GetExtensibleThemePluginsService,
            GetExtensibleThemePlugins.Repository.IGetExtensibleThemePluginsRepository, GetExtensibleThemePlugins.Repository.GetExtensibleThemePluginsRepository>(services);

        return services;
    }

    private static void Register<TService, TServiceImpl, TRepository, TRepositoryImpl>(IServiceCollection services)
        where TService : class
        where TServiceImpl : class, TService
        where TRepository : class
        where TRepositoryImpl : class, TRepository
    {
        services.AddScoped<TService, TServiceImpl>();
        services.AddTransient<TRepository, TRepositoryImpl>();
    }

    private static void RegisterExtensibleTheme<TService, TServiceImpl, TRepository, TRepositoryImpl>(IServiceCollection services)
        where TService : class
        where TServiceImpl : class, TService
        where TRepository : class
        where TRepositoryImpl : class, TRepository
    {
        services.AddScoped<TService, TServiceImpl>();
        services.AddTransient<TRepository, TRepositoryImpl>();
    }
}
