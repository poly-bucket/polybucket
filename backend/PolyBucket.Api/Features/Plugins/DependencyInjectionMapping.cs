using Microsoft.Extensions.DependencyInjection;
using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Features.Plugins.Services;

namespace PolyBucket.Api.Features.Plugins;

public static class PluginsDependencyInjectionMapping
{
    public static IServiceCollection AddPluginsFeature(this IServiceCollection services)
    {
        services.AddSingleton<PluginManager>(provider =>
        {
            var pluginsPath = Path.Combine(Directory.GetCurrentDirectory(), "plugins");
            return new PluginManager(pluginsPath);
        });

        services.AddScoped<IMarketplaceBaseUrlResolver, MarketplaceBaseUrlResolver>();
        services.AddHttpClient<IPluginArchiveDownloader, GitHubPluginArchiveDownloader>();
        services.AddHttpClient<MarketplaceClient>();
        services.AddScoped<PluginManifestValidator>();
        services.AddScoped<PluginInstallationService>();
        services.AddScoped<OAuthPluginService>();
        services.AddScoped<MetadataPluginService>();
        services.AddScoped<ThemePluginService>();

        RegisterAction<GetPlugins.Domain.IGetPluginsService, GetPlugins.Domain.GetPluginsService,
            GetPlugins.Repository.IGetPluginsRepository, GetPlugins.Repository.GetPluginsRepository>(services);
        RegisterAction<ReloadPlugins.Domain.IReloadPluginsService, ReloadPlugins.Domain.ReloadPluginsService,
            ReloadPlugins.Repository.IReloadPluginsRepository, ReloadPlugins.Repository.ReloadPluginsRepository>(services);
        RegisterAction<GetInstalledPlugins.Domain.IGetInstalledPluginsService, GetInstalledPlugins.Domain.GetInstalledPluginsService,
            GetInstalledPlugins.Repository.IGetInstalledPluginsRepository, GetInstalledPlugins.Repository.GetInstalledPluginsRepository>(services);
        RegisterAction<InstallPlugin.Domain.IInstallPluginService, InstallPlugin.Domain.InstallPluginService,
            InstallPlugin.Repository.IInstallPluginRepository, InstallPlugin.Repository.InstallPluginRepository>(services);
        RegisterAction<UninstallPlugin.Domain.IUninstallPluginService, UninstallPlugin.Domain.UninstallPluginService,
            UninstallPlugin.Repository.IUninstallPluginRepository, UninstallPlugin.Repository.UninstallPluginRepository>(services);
        RegisterAction<GetPluginHealth.Domain.IGetPluginHealthService, GetPluginHealth.Domain.GetPluginHealthService,
            GetPluginHealth.Repository.IGetPluginHealthRepository, GetPluginHealth.Repository.GetPluginHealthRepository>(services);
        RegisterAction<GetMarketplacePlugins.Domain.IGetMarketplacePluginsService, GetMarketplacePlugins.Domain.GetMarketplacePluginsService,
            GetMarketplacePlugins.Repository.IGetMarketplacePluginsRepository, GetMarketplacePlugins.Repository.GetMarketplacePluginsRepository>(services);
        RegisterAction<GetMarketplacePluginDetails.Domain.IGetMarketplacePluginDetailsService, GetMarketplacePluginDetails.Domain.GetMarketplacePluginDetailsService,
            GetMarketplacePluginDetails.Repository.IGetMarketplacePluginDetailsRepository, GetMarketplacePluginDetails.Repository.GetMarketplacePluginDetailsRepository>(services);
        RegisterAction<GetMarketplaceCategories.Domain.IGetMarketplaceCategoriesService, GetMarketplaceCategories.Domain.GetMarketplaceCategoriesService,
            GetMarketplaceCategories.Repository.IGetMarketplaceCategoriesRepository, GetMarketplaceCategories.Repository.GetMarketplaceCategoriesRepository>(services);
        RegisterAction<InstallMarketplacePlugin.Domain.IInstallMarketplacePluginService, InstallMarketplacePlugin.Domain.InstallMarketplacePluginService,
            InstallMarketplacePlugin.Repository.IInstallMarketplacePluginRepository, InstallMarketplacePlugin.Repository.InstallMarketplacePluginRepository>(services);
        RegisterAction<GetMarketplacePluginInstallationStatus.Domain.IGetMarketplacePluginInstallationStatusService, GetMarketplacePluginInstallationStatus.Domain.GetMarketplacePluginInstallationStatusService,
            GetMarketplacePluginInstallationStatus.Repository.IGetMarketplacePluginInstallationStatusRepository, GetMarketplacePluginInstallationStatus.Repository.GetMarketplacePluginInstallationStatusRepository>(services);
        RegisterAction<GetPluginDetails.Domain.IGetPluginDetailsService, GetPluginDetails.Domain.GetPluginDetailsService,
            GetPluginDetails.Repository.IGetPluginDetailsRepository, GetPluginDetails.Repository.GetPluginDetailsRepository>(services);
        RegisterAction<GetPluginsOverview.Domain.IGetPluginsOverviewService, GetPluginsOverview.Domain.GetPluginsOverviewService,
            GetPluginsOverview.Repository.IGetPluginsOverviewRepository, GetPluginsOverview.Repository.GetPluginsOverviewRepository>(services);
        RegisterAction<GetPluginHooks.Domain.IGetPluginHooksService, GetPluginHooks.Domain.GetPluginHooksService,
            GetPluginHooks.Repository.IGetPluginHooksRepository, GetPluginHooks.Repository.GetPluginHooksRepository>(services);
        RegisterAction<UpdatePluginSettings.Domain.IUpdatePluginSettingsService, UpdatePluginSettings.Domain.UpdatePluginSettingsService,
            UpdatePluginSettings.Repository.IUpdatePluginSettingsRepository, UpdatePluginSettings.Repository.UpdatePluginSettingsRepository>(services);
        RegisterAction<UpdatePluginStatus.Domain.IUpdatePluginStatusService, UpdatePluginStatus.Domain.UpdatePluginStatusService,
            UpdatePluginStatus.Repository.IUpdatePluginStatusRepository, UpdatePluginStatus.Repository.UpdatePluginStatusRepository>(services);
        RegisterAction<GetPluginSettings.Domain.IGetPluginSettingsService, GetPluginSettings.Domain.GetPluginSettingsService,
            GetPluginSettings.Repository.IGetPluginSettingsRepository, GetPluginSettings.Repository.GetPluginSettingsRepository>(services);
        RegisterAction<GetMarketplaceBaseUrl.Domain.IGetMarketplaceBaseUrlService, GetMarketplaceBaseUrl.Domain.GetMarketplaceBaseUrlService,
            GetMarketplaceBaseUrl.Repository.IGetMarketplaceBaseUrlRepository, GetMarketplaceBaseUrl.Repository.GetMarketplaceBaseUrlRepository>(services);
        RegisterAction<UpdateMarketplaceBaseUrl.Domain.IUpdateMarketplaceBaseUrlService, UpdateMarketplaceBaseUrl.Domain.UpdateMarketplaceBaseUrlService,
            UpdateMarketplaceBaseUrl.Repository.IUpdateMarketplaceBaseUrlRepository, UpdateMarketplaceBaseUrl.Repository.UpdateMarketplaceBaseUrlRepository>(services);
        RegisterAction<GetInstalledPluginAsset.Domain.IGetInstalledPluginAssetService, GetInstalledPluginAsset.Domain.GetInstalledPluginAssetService,
            GetInstalledPluginAsset.Repository.IGetInstalledPluginAssetRepository, GetInstalledPluginAsset.Repository.GetInstalledPluginAssetRepository>(services);
        RegisterAction<GetOAuthProviders.Domain.IGetOAuthProvidersService, GetOAuthProviders.Domain.GetOAuthProvidersService,
            GetOAuthProviders.Repository.IGetOAuthProvidersRepository, GetOAuthProviders.Repository.GetOAuthProvidersRepository>(services);
        RegisterAction<GetOAuthAuthorizationUrl.Domain.IGetOAuthAuthorizationUrlService, GetOAuthAuthorizationUrl.Domain.GetOAuthAuthorizationUrlService,
            GetOAuthAuthorizationUrl.Repository.IGetOAuthAuthorizationUrlRepository, GetOAuthAuthorizationUrl.Repository.GetOAuthAuthorizationUrlRepository>(services);
        RegisterAction<HandleOAuthCallback.Domain.IHandleOAuthCallbackService, HandleOAuthCallback.Domain.HandleOAuthCallbackService,
            HandleOAuthCallback.Repository.IHandleOAuthCallbackRepository, HandleOAuthCallback.Repository.HandleOAuthCallbackRepository>(services);
        RegisterAction<GetOAuthUserInfo.Domain.IGetOAuthUserInfoService, GetOAuthUserInfo.Domain.GetOAuthUserInfoService,
            GetOAuthUserInfo.Repository.IGetOAuthUserInfoRepository, GetOAuthUserInfo.Repository.GetOAuthUserInfoRepository>(services);
        RegisterAction<RefreshOAuthToken.Domain.IRefreshOAuthTokenService, RefreshOAuthToken.Domain.RefreshOAuthTokenService,
            RefreshOAuthToken.Repository.IRefreshOAuthTokenRepository, RefreshOAuthToken.Repository.RefreshOAuthTokenRepository>(services);
        RegisterAction<ValidateOAuthToken.Domain.IValidateOAuthTokenService, ValidateOAuthToken.Domain.ValidateOAuthTokenService,
            ValidateOAuthToken.Repository.IValidateOAuthTokenRepository, ValidateOAuthToken.Repository.ValidateOAuthTokenRepository>(services);
        RegisterAction<GetMetadataPlugins.Domain.IGetMetadataPluginsService, GetMetadataPlugins.Domain.GetMetadataPluginsService,
            GetMetadataPlugins.Repository.IGetMetadataPluginsRepository, GetMetadataPlugins.Repository.GetMetadataPluginsRepository>(services);
        RegisterAction<GetMetadataEntityFields.Domain.IGetMetadataEntityFieldsService, GetMetadataEntityFields.Domain.GetMetadataEntityFieldsService,
            GetMetadataEntityFields.Repository.IGetMetadataEntityFieldsRepository, GetMetadataEntityFields.Repository.GetMetadataEntityFieldsRepository>(services);
        RegisterAction<ValidateMetadata.Domain.IValidateMetadataService, ValidateMetadata.Domain.ValidateMetadataService,
            ValidateMetadata.Repository.IValidateMetadataRepository, ValidateMetadata.Repository.ValidateMetadataRepository>(services);
        RegisterAction<TransformMetadata.Domain.ITransformMetadataService, TransformMetadata.Domain.TransformMetadataService,
            TransformMetadata.Repository.ITransformMetadataRepository, TransformMetadata.Repository.TransformMetadataRepository>(services);
        RegisterAction<GetDefaultMetadata.Domain.IGetDefaultMetadataService, GetDefaultMetadata.Domain.GetDefaultMetadataService,
            GetDefaultMetadata.Repository.IGetDefaultMetadataRepository, GetDefaultMetadata.Repository.GetDefaultMetadataRepository>(services);
        RegisterAction<GetMetadataSchema.Domain.IGetMetadataSchemaService, GetMetadataSchema.Domain.GetMetadataSchemaService,
            GetMetadataSchema.Repository.IGetMetadataSchemaRepository, GetMetadataSchema.Repository.GetMetadataSchemaRepository>(services);
        RegisterAction<GetActiveThemes.Domain.IGetActiveThemesService, GetActiveThemes.Domain.GetActiveThemesService,
            GetActiveThemes.Repository.IGetActiveThemesRepository, GetActiveThemes.Repository.GetActiveThemesRepository>(services);
        RegisterAction<ApplyTheme.Domain.IApplyThemeService, ApplyTheme.Domain.ApplyThemeService,
            ApplyTheme.Repository.IApplyThemeRepository, ApplyTheme.Repository.ApplyThemeRepository>(services);
        RegisterAction<RemoveTheme.Domain.IRemoveThemeService, RemoveTheme.Domain.RemoveThemeService,
            RemoveTheme.Repository.IRemoveThemeRepository, RemoveTheme.Repository.RemoveThemeRepository>(services);
        RegisterAction<UpdateThemeSettings.Domain.IUpdateThemeSettingsService, UpdateThemeSettings.Domain.UpdateThemeSettingsService,
            UpdateThemeSettings.Repository.IUpdateThemeSettingsRepository, UpdateThemeSettings.Repository.UpdateThemeSettingsRepository>(services);
        RegisterAction<GetThemeSettings.Domain.IGetThemeSettingsService, GetThemeSettings.Domain.GetThemeSettingsService,
            GetThemeSettings.Repository.IGetThemeSettingsRepository, GetThemeSettings.Repository.GetThemeSettingsRepository>(services);

        return services;
    }

    private static void RegisterAction<TService, TServiceImpl, TRepository, TRepositoryImpl>(IServiceCollection services)
        where TService : class
        where TServiceImpl : class, TService
        where TRepository : class
        where TRepositoryImpl : class, TRepository
    {
        services.AddScoped<TService, TServiceImpl>();
        services.AddTransient<TRepository, TRepositoryImpl>();
    }
}
